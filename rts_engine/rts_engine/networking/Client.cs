using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RtsEngine.Networking
{

public class Client
{
	private const int _MAX_DATA_LENGTH = 10 * 1024 * 1024;

	private readonly byte[] _buffer;
	private TcpClient _client;
	private NetworkStream? _stream;
	private readonly object _streamLock;

	public event EventHandler<byte[]>? MessageReceived;

	public Client()
	{
		_client = new TcpClient();
		_buffer = new byte[8192];
		_stream = null;
		_streamLock = new object();
	}

	public async Task ConnectAsync(string host, int port)
	{
		await _client.ConnectAsync(host, port);
		_stream = _client.GetStream();

		Console.WriteLine($"Connected to {host}:{port}");

		_ = ListenAsync();
	}

	private async Task ListenAsync()
	{
		try
		{
			while (_client.Connected)
			{
				byte[] lengthBytes = new byte[4];
				int bytesRead = await ReadFullAsync(lengthBytes, 4);
				if (bytesRead == 0) break;

				int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBytes, 0));

				if (length > _MAX_DATA_LENGTH)
				{
					Console.WriteLine($"Message too large ({length}) bytes");
					break;
				}

				byte[] data = new byte[length];
				bytesRead = await ReadFullAsync(data, length);
				if (bytesRead == 0) break;

				Console.WriteLine($"Received {length} bytes");
				OnMessageReceived(data);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Connection lost: {ex.Message}");
		}
		finally
		{
			Disconnect();
		}
	}

	private async Task<int> ReadFullAsync(byte[] buffer, int count)
	{
		int totalRead = 0;
		while (totalRead < count)
		{
			int read = await _stream!.ReadAsync(buffer, totalRead, count - totalRead);
			if (read == 0) break;
			totalRead += read;
		}
		return totalRead;
	}

	public async Task SendAsync(byte[] data)
	{
		if (!_client.Connected)
		{
			throw new InvalidOperationException("Not connected to server");
		}

		lock (_streamLock)
		{
			byte[] lengthBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
			_stream!.Write(lengthBytes, 0, 4);
			_stream!.Write(data, 0, data.Length);
		}
	}

	private void OnMessageReceived(byte[] data)
	{
		MessageReceived?.Invoke(this, data);
	}

	public void Disconnect()
	{
		_stream?.Close();
		_client?.Close();
		Console.WriteLine("Disconnected");
	}
}
}
