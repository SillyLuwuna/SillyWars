using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RtsEngine.Networking
{

public class Server
{
	private const int _MAX_DATA_LENGTH = 10 * 1024 * 1024;

	private readonly TcpListener _listener;
	private readonly List<TcpClient> _clients;
	private readonly object _clientsLock;
	private readonly int _requiredClients;

	private bool _isRunning;
	public event EventHandler<byte[]>? MessageReceived;
	public event EventHandler<TcpClient>? ConnectionEstablished;

	public Server(int port, int requiredClients)
	{
		_listener = new TcpListener(IPAddress.Any, port);
		_clients = new List<TcpClient>();
		_clientsLock = new object();
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

	private async Task HandleClientAsync(TcpClient client)
	{
		try
		{
			using NetworkStream stream = client.GetStream();

			while (client.Connected)
			{
				byte[] lengthBytes = new byte[4];
				int bytesRead = await ReadFullAsync(stream, lengthBytes, 4);
				if (bytesRead == 0) break;

				int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBytes, 0));

				if (length > _MAX_DATA_LENGTH)
				{
					Console.WriteLine($"Message too large ({length}) bytes");
					break;
				}

				byte[] data = new byte[length];
				bytesRead = await ReadFullAsync(stream, data, length);
				if (bytesRead == 0) break;

				Console.WriteLine($"Received {length} bytes");
				OnMessageReceived(data);
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
		lock (_clientsLock)
		{
			TcpClient client = _clients[id];
			try
			{
				NetworkStream stream = client.GetStream();

				if (data.Length > _MAX_DATA_LENGTH)
				{
					Console.WriteLine($"Message too large ({data.Length}) bytes");
					return;
				}

				byte[] lengthBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
				stream.Write(lengthBytes, 0, 4);
				stream.Write(data, 0, data.Length);
			}
			catch (Exception ex)
			{
				// client may have disconnected
				Console.WriteLine($"Error sending: {ex.Message}");
			}
		}
	}

	private void HandleConnection(TcpClient client)
	{
		lock (_clientsLock)
		{
			if (_clients.Count >= _requiredClients)
			{
				client.Close();
				Console.WriteLine($"Rejected connection ({_clients.Count}/{_requiredClients})");
				return;
			}

			_clients.Add(client);
		}

		Console.WriteLine($"Accepted connection ({_clients.Count}/{_requiredClients})");
		OnConnectionEstablished(client);
		_ = HandleClientAsync(client);
	}

	private void HandleDisconnection(TcpClient client)
	{
		lock (_clientsLock)
		{
			_clients.Remove(client);
		}
		client.Close();
		Console.WriteLine($"Client disconnected ({_clients.Count}/{_requiredClients})");
	}

	public void Stop()
	{
		_isRunning = false;
		lock (_clientsLock)
		{
			foreach (TcpClient client in _clients)
			{
				client.Close();
			}
			_clients.Clear();
		}
		_listener.Stop();
		Console.WriteLine("Server stopped");
	}

	private void OnMessageReceived(byte[] data)
	{
		MessageReceived?.Invoke(this, data);
	}

	private void OnConnectionEstablished(TcpClient client)
	{
		// IPEndPoint endPoint = client.Client.RemoteEndPoint as IPEndPoint;
		ConnectionEstablished?.Invoke(this, client);
	}
}
}
