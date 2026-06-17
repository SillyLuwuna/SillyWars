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

public class GameServer : MonoBehaviour
{
	public string Ip = "localhost";
	public int Port = 13774;
	public int TimeoutMs = 1000;

	private Client? _client;
	private readonly ConcurrentQueue<byte[]> _dataQueue = new ConcurrentQueue<byte[]>();
	// private byte[]? _data;
	// private readonly object _dataLock = new object();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		Console.SetOut(new UnityTextWriter());
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
		_client = new Client(TimeoutMs);
		_client.MessageReceived += HandleData;

		Task connectTask = _client.ConnectAsync(Ip, Port);
		yield return new WaitUntil(() => connectTask.IsCompleted);

		try
		{
			connectTask.GetAwaiter().GetResult();
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
		while (_dataQueue.TryDequeue(out byte[]? data))
		{
			ProcessData(data);
		}
    }

	private void ProcessData(byte[] data)
	{
		Debug.Log($"Received {data.Length} bytes");

		byte[] decompressedData = DataCompressor.DecompressData(data);
		WorldState state = Serializer.FromBytes<WorldState>(decompressedData);
		Console.WriteLine(state.Map.Size());
	}

	private void HandleData(object? sender, byte[] data)
	{
		_dataQueue.Enqueue(data);
		// lock (_dataLock)
		// {
		// 	_data = data;
		// }
	}

	void OnDestroy()
	{
		_client?.Disconnect();
	}
}
