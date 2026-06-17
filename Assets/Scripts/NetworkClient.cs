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

public class NetworkClient : MonoBehaviour
{
	private static NetworkClient? _instance = null;
	private static bool _awoken = false;

	public string Ip = "localhost";
	public int Port = 13774;
	public int TimeoutMs = 1000;

	private Client _client = null!;
	private readonly ConcurrentQueue<WorldState> _dataQueue = new ConcurrentQueue<WorldState>();

	public event EventHandler<WorldState>? Tick;
	public event Action? ConnectionLost;
	public event Action? ConnectionEstablished;


	private NetworkClient() { }

	public static NetworkClient Instance()
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
    }

	public void TryConnect()
	{
		StartCoroutine(ConnectToServer());
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
		if (!_client.IsConnected) return;

		while (_dataQueue.TryDequeue(out WorldState state))
		{
			OnSimulationTick(state);
		}
    }

	private void HandleData(object? sender, byte[] data)
	{
		// Debug.Log($"Received {data.Length} bytes");

		byte[] decompressedData = DataCompressor.DecompressData(data);
		WorldState state = Serializer.FromBytes<WorldState>(decompressedData);

		_dataQueue.Enqueue(state);
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

	public bool IsConnected()
	{
		return _client.IsConnected;
	}

	public void Disconnect()
	{
		_client.Disconnect();
	}
}
