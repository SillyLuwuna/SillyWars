using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RtsEngine.Networking
{

public class DataEventArgs : EventArgs
{
	public byte[] Data { get; private set; }
	public string Ip { get; private set; }
	public int Port { get; private set; }

	public DataEventArgs(byte[] data, string ip, int port) : base()
	{
		Data = data;
		Ip = ip;
		Port = port;
	}
}

public class CustomTcpClient
{
	public readonly TcpClient Client;
	public readonly string Ip;
	public readonly int Port;

	public CustomTcpClient(TcpClient client)
	{
		Client = client;

		IPEndPoint? clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
		Ip = clientEndPoint!.Address.ToString();
		Port = clientEndPoint.Port;
	}

	public string Endpoint
	{
		get => $"{Ip}{Port}";
	}

	public static string GenerateEndpoint(string ip, int port)
	{
		return $"{ip}{port}";
	}
}

public class Server
{
	private const int MaxDataLength = 10 * 1024 * 1024;

	private readonly TcpListener _listener;
	private readonly List<CustomTcpClient> _clients;
	private readonly Dictionary<string, CustomTcpClient> _clientEndpoints;
	// private readonly List<TcpClient> _clients;
	// private readonly Dictionary<string, TcpClient> _clientEndpoints;
	// private readonly object _clientsLock;
	private readonly SemaphoreSlim _clientsSemaphore;
	private readonly int _requiredClients;

	private bool _isRunning;
	public event EventHandler<DataEventArgs>? MessageReceived;
	public event EventHandler<DataEventArgs>? ConnectionEstablished;
	public event EventHandler<DataEventArgs>? ConnectionLost;

	public Server(int port, int requiredClients)
	{
		_listener = new TcpListener(IPAddress.Any, port);
		_listener.Server.NoDelay = true;
		_clients = new List<CustomTcpClient>();
		_clientEndpoints = new Dictionary<string, CustomTcpClient>();
		// _clientsLock = new object();
		_clientsSemaphore = new SemaphoreSlim(1, 1);
		_requiredClients = requiredClients;
		_isRunning = false;
	}

	public async Task StartAsync()
	{
		_listener.Start();
		_isRunning = true;
		Console.WriteLine($"Server started on port {((IPEndPoint)_listener.LocalEndpoint).Port}");
		Console.WriteLine($"Waiting for up to {_requiredClients} clients...");

		// while (_clients.Count < _requiredClients)
		while (_isRunning)
		{
			TcpClient client = await _listener.AcceptTcpClientAsync();

			HandleConnection(client);
		}
	}

	private async Task HandleClientAsync(CustomTcpClient client)
	{
		try
		{
			using NetworkStream stream = client.Client.GetStream();

			while (client.Client.Connected)
			{
				byte[] lengthBytes = new byte[4];
				int bytesRead = await ReadFullAsync(stream, lengthBytes, 4);
				if (bytesRead == 0) break;

				int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBytes, 0));

				if (length > MaxDataLength)
				{
					Console.WriteLine($"Message too large ({length}) bytes");
					break;
				}

				byte[] data = new byte[length];
				bytesRead = await ReadFullAsync(stream, data, length);
				if (bytesRead == 0) break;

				OnMessageReceived(data, client);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Client error: {ex.Message}");
		}
		finally
		{
			HandleDisconnection(client);
		}
	}

	private async Task<int> ReadFullAsync(NetworkStream stream, byte[] buffer, int count)
	{
		int totalRead = 0;
		while (totalRead < count)
		{
			int read = await stream.ReadAsync(buffer, totalRead, count - totalRead);
			if (read == 0) break;
			totalRead += read;
		}
		return totalRead;
	}

	public async Task SendData(byte[] data, int id)
	{
		await _clientsSemaphore.WaitAsync();

		await TrySendData(data, _clients[id]);
		_clientsSemaphore.Release();
	}

	public async Task SendData(byte[] data, string endpoint)
	{
		await _clientsSemaphore.WaitAsync();

		if (!_clientEndpoints.ContainsKey(endpoint)) return;

		await TrySendData(data, _clientEndpoints[endpoint]);
		_clientsSemaphore.Release();
	}

	public async Task BroadcastData(byte[] data)
	{
		await _clientsSemaphore.WaitAsync();
		foreach (CustomTcpClient client in _clients)
		{
			await TrySendData(data, client);
		}
		_clientsSemaphore.Release();
	}

	private async Task TrySendData(byte[] data, CustomTcpClient client)
	{
		try
		{
			NetworkStream stream = client.Client.GetStream();

			if (data.Length > MaxDataLength)
			{
				Console.WriteLine($"Message too large ({data.Length}) bytes");
				return;
			}

			byte[] lengthBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
			await stream.WriteAsync(lengthBytes, 0, 4);
			await stream.WriteAsync(data, 0, data.Length);
		}
		catch (Exception ex)
		{
			// client may have disconnected
			Console.WriteLine($"Error sending: {ex.Message}");
		}
	}

	private void HandleConnection(TcpClient client)
	{
		CustomTcpClient customClient = new CustomTcpClient(client);

		_clientsSemaphore.Wait();
		try
		{
			if (_clients.Count >= _requiredClients)
			{
				client.Close();
				Console.WriteLine($"Rejected connection ({_clients.Count}/{_requiredClients})");
				return;
			}

			_clients.Add(customClient);
			_clientEndpoints.Add(customClient.Endpoint, customClient);
		}
		finally
		{
			_clientsSemaphore.Release();
		}

		Console.WriteLine($"Accepted connection ({_clients.Count}/{_requiredClients})");
		OnConnectionEstablished(customClient);
		_ = HandleClientAsync(customClient);
	}

	private void HandleDisconnection(CustomTcpClient client)
	{
		_clientsSemaphore.Wait();
		try
		{
			_clients.Remove(client);
			_clientEndpoints.Remove(client.Endpoint);
		}
		finally
		{
			_clientsSemaphore.Release();
		}
		Console.WriteLine($"Client disconnected ({_clients.Count}/{_requiredClients})");
		OnConnectionLost(client);
		client.Client.Close();
	}

	public void Stop()
	{
		if (!_isRunning) return;

		_isRunning = false;

		_clientsSemaphore.Wait();
		try
		{
			foreach (CustomTcpClient client in _clients)
			{
				client.Client.Close();
			}
			_clients.Clear();
			_clientEndpoints.Clear();
		}
		finally
		{
			_clientsSemaphore.Release();
		}

		_listener.Stop();
		Console.WriteLine("Server stopped");
	}

	private void OnMessageReceived(byte[] data, CustomTcpClient client)
	{
		// IPEndPoint? clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
		// DataEventArgs dataArgs = new DataEventArgs(data, clientEndPoint!.Address.ToString(), clientEndPoint.Port);
		DataEventArgs dataArgs = new DataEventArgs(data, client.Ip, client.Port);

		MessageReceived?.Invoke(this, dataArgs);
	}

	private void OnConnectionEstablished(CustomTcpClient client)
	{
		// IPEndPoint? clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
		// DataEventArgs dataArgs = new DataEventArgs(null!, clientEndPoint!.Address.ToString(), clientEndPoint.Port);
		DataEventArgs dataArgs = new DataEventArgs(null!, client.Ip, client.Port);

		ConnectionEstablished?.Invoke(this, dataArgs);
	}

	private void OnConnectionLost(CustomTcpClient client)
	{
		// IPEndPoint? clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
		// DataEventArgs dataArgs = new DataEventArgs(null!, clientEndPoint!.Address.ToString(), clientEndPoint.Port);
		DataEventArgs dataArgs = new DataEventArgs(null!, client.Ip, client.Port);

		ConnectionLost?.Invoke(this, dataArgs);
	}

	public int ConnectionCount
	{
		get => _clients.Count;
	}
}
}
