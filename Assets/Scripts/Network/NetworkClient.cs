#nullable enable

using UnityEngine;
using RtsEngine;
using RtsEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using System.Text;
using System.IO;
using RtsEngine.Data;
using RtsEngine.Commands;

public class NetworkClient : MonoBehaviour
{
	public static int SERVER_TPS = 20; // value should be obtained from the server

	private static NetworkClient? _instance = null;
	private static bool _awoken = false;

	public string Ip = "localhost";
	public int Port = 13774;
	public int TimeoutMs = 1000;

	private Client _client = null!;
	// private readonly ConcurrentQueue<WorldState> _dataQueue = new ConcurrentQueue<WorldState>();
	private WorldState _currState = null!;
	private object _stateLock = new object();
	private bool _update = false;

	public event EventHandler<WorldState>? Tick;
	public event Action? ConnectionLost;
	public event Action? ConnectionEstablished;


	private NetworkClient() { }

	public static NetworkClient Instance
	{
		get
		{
			if (!_awoken || _instance == null)
			{
				throw new MethodAccessException("Instance was not initialized yet");
			}

			// if (_instance == null)
			// {
			// 	_instance = new NetworkClient();
			// }

			return _instance;
		}
	}

	void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
			return;
		}

		_instance = this;
		DontDestroyOnLoad(gameObject);
		_awoken = true;

		Console.SetOut(new UnityTextWriter());

		StartClient();
	}

	private void StartClient()
	{
		_client = new Client(TimeoutMs);
		_client.MessageReceived += HandleData;
		_client.Connection += OnConnectionEstablished;
		_client.Disconnection += OnConnectionLost;
	}

    void Start()
    {
		if (this != _instance) return;

		LocalEngine.Instance.Started += OnLocalStart;
		LocalEngine.Instance.TickEnded += OnLocalStateUpdate;
		LocalEngine.Instance.Stopped += OnLocalStop;
    }

	public void TryConnect(string ip, int port)
	{
		Ip = ip;
		Port = port;
		StartCoroutine(ConnectToServer());
	}

	private void OnLocalStateUpdate(object? sender, WorldState state)
	{
		// Debug.Log($"{LocalEngine.Instance.Engine?.TPS}");
		// byte[] stateData = Serializer.ToBytes(state);
		// WorldState copy = Serializer.FromBytes<WorldState>(stateData);
		state.SetPlayerVersion(0);
		UpdateState(state);
	}

	private void OnLocalStart()
	{
		OnConnectionEstablished(); // error prone when classes think they are subscribing to connections
	}

	private void OnLocalStop()
	{
		OnConnectionLost(); // error prone when classes think they are subscribing to connections
	}

	private class UnityTextWriter : TextWriter
	{
		public override Encoding Encoding => Encoding.UTF8;

		public override void Write(string value)
		{
			Debug.Log(value);
		}

		public override void WriteLine(string value)
		{
			Debug.Log(value);
		}
	}

	private IEnumerator ConnectToServer()
	{
		Task connectTask = _client.ConnectAsync(Ip, Port);
		yield return new WaitUntil(() => connectTask.IsCompleted);

		try
		{
			connectTask.GetAwaiter().GetResult();
			if (connectTask.IsFaulted)
			{
				throw connectTask.Exception;
			}
			Debug.Log("Connected to server!");
		}
		catch (Exception ex)
		{
            Debug.LogError($"Connection failed: {ex}");
		}
	}

    // Update is called once per frame
    void Update()
    {
		if (!_client.IsConnected && !IsLocalInstance) return;

		lock (_stateLock)
		{
			if (!_update) return;

			_update = false;
			OnSimulationTick(_currState);
		}
    }

	private bool IsLocalInstance => LocalEngine.Instance.IsRunning;

	public void SendCommand(ICommand command)
	{
		if (IsLocalInstance)
		{
			LocalEngine.Instance.Engine?.EnqueueCommand(command);
			return;
		}

		byte[] data = Serializer.ToBytes(command);
		byte[] compressedData = DataCompressor.CompressData(data);
		// TODO dangerous
		_ = _client.SendAsync(compressedData);
	}

	private void HandleData(object? sender, byte[] data)
	{
		try
		{
			byte[] decompressedData = DataCompressor.DecompressData(data);
			WorldState state = Serializer.FromBytes<WorldState>(decompressedData);
			UpdateState(state);
		}
		catch (Exception ex)
		{
			Debug.Log($"error reading data: {ex.Message}\n{ex.StackTrace}");
		}

	}

	private void UpdateState(WorldState state)
	{
		lock (_stateLock)
		{
			_currState = state;

			_update = true;
		}
	}

	void OnDestroy()
	{
		_client?.Disconnect();
	}

	private void OnSimulationTick(WorldState state)
	{
		Tick?.Invoke(this, state);
	}

	private void OnConnectionLost()
	{
		ConnectionLost?.Invoke();
	}

	private void OnConnectionEstablished()
	{
		ConnectionEstablished?.Invoke();
	}

	public bool IsConnected => _client.IsConnected;

	public void Disconnect()
	{
		_client.Disconnect();
	}
}
