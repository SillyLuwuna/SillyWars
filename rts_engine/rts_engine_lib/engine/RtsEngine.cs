using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RtsEngine.Commands;
using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Networking;
using RtsEngine.Physics;

namespace RtsEngine
{

public class RtsEngine
{
	private static RtsEngine? _instance;

	public const int TPS = 20;
	private const int INTERVAL_MS = 1000 / TPS;
	private const int PORT = 13774;

	private const int STAT_INTERVAL_MS = 1000;
	private const bool SHOW_STATS = true;
	
	private const int NUM_PLAYERS = 2;

	public bool IsRunning { get; private set; }
	private PhysicsEngine _physicsEngine;
	private WorldState _state;
	private Clock _clock;
	private Server _server;
	private Dictionary<string, uint> _playerIds;
	private Dictionary<uint, string> _playerEndpoints;

	private object _commandQueueLock;
	private Queue<ICommand> _commandQueue;

	private ulong _totalTicks;
	private int _statInterval;
	private float _statLoadSum;
	private int _statTicks;
	private long _statByteSum;
	private long _statByteIncSum;
	private int _statPacketsReceived;
	private int _statPacketsSent;

	private Task? _currentBroadcastTask;
	private readonly object _broadcastLock;

	public static RtsEngine StartInstance(WorldState state)
	{
		_instance = new RtsEngine(state);
		return _instance;
	}

	public static RtsEngine Instance
	{
		get => _instance!;
	}

	private RtsEngine(WorldState state)
	{
		_physicsEngine = new PhysicsEngine();
		_currentBroadcastTask = null;
		_broadcastLock = new object();
		_playerIds = new Dictionary<string, uint>();
		_playerEndpoints = new Dictionary<uint, string>();
		_commandQueue = new Queue<ICommand>();
		_commandQueueLock = new object();
		IsRunning = false;
		_state = state;
		_clock = new Clock(INTERVAL_MS);
		_clock.Tick += TickSubscriber;
		_server = new Server(PORT, NUM_PLAYERS);
		_server.MessageReceived += OnDataReceived;
		_server.ConnectionEstablished += OnConnectionEstablished;
		Reset();
	}

	public WorldState State { get => _state; }

	private void Reset()
	{
		_totalTicks = 0;
		_statInterval = 0;
		_statLoadSum = 0.0f;
		_statTicks = 0;
		_statByteSum = 0;
		_statByteIncSum = 0;
		_statPacketsReceived = 0;
		_statPacketsSent = 0;
		_playerEndpoints.Clear();
		_playerIds.Clear();

		lock(_commandQueueLock)
		{
			_commandQueue.Clear();
		}
	}

	public async Task Start()
	{
		Console.WriteLine("Starting engine...");
		IsRunning = true;
		_ = _server.StartAsync();
		_clock.Start();
		Console.WriteLine("Engine started.");
	}

	public void Stop()
	{
		Console.WriteLine("Stopping engine...");
		IsRunning = false;
		_clock.StopAsync().GetAwaiter().GetResult();
		_server.Stop();
		Reset();
		Console.WriteLine("Engine stopped.");
	}

	public void Tick()
	{
		if (!IsRunning) throw new InvalidOperationException("cannot tick when the engine is not running.");
		_totalTicks++;

		WaitForPreviousTickBroadcast();
		CleanupDestroyedEntities();
		ExecutePlayerCommands();
		UpdateWorldState();
		UpdatePhysics();
		BroadcastWorldState();
	}

	private void WaitForPreviousTickBroadcast()
	{
		lock (_broadcastLock)
		{
			if (_currentBroadcastTask != null && !_currentBroadcastTask.IsCompleted)
			{
				Console.WriteLine("Network bottleneck.");
				_currentBroadcastTask.Wait();
			}
		}
	}

	private void ExecutePlayerCommands()
	{
		lock(_commandQueueLock)
		{
			while(_commandQueue.Count > 0)
			{
				ICommand command = _commandQueue.Dequeue();
				command.Execute(_state);
			}
		}
	}

	private void UpdateWorldState()
	{
		_state.Tick();
	}

	private void CleanupDestroyedEntities()
	{
		_state.CleanupDestroyedEntities();
	}

	private void UpdatePhysics()
	{
		List<PhysicsObject> physicsObjects = _state.PhysicsObjects;

		_physicsEngine.ProcessCollisions(physicsObjects);
		_physicsEngine.PhysicsTick(physicsObjects);
		_physicsEngine.LimitToMapBoundaries(physicsObjects, _state.Map);
	}

	private void BroadcastWorldState()
	{
		byte[] data = Serializer.ToBytes(_state);
		byte[] compressedData = DataCompressor.CompressData(data);

		_currentBroadcastTask = _server.BroadcastData(compressedData);


		_statByteSum += compressedData.Length * _server.ConnectionCount;
		_statPacketsSent += _server.ConnectionCount;
	}

	private void TickSubscriber(object? sender, ClockEventArgs e)
	{
		CalcStats(e.DeltaTime, e.Load, e.ElapsedMs);
		try
		{
			Tick();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error during tick: {ex.Message}");
		}

	}

	private string GetPlayerEndpoint(string ip, int port)
	{
		return $"{ip}{port}";
	}

	private void OnDataReceived(object? sender, DataEventArgs args)
	{
		_statByteIncSum += args.Data.Length;
		_statPacketsReceived++;

		uint playerId = _playerIds[GetPlayerEndpoint(args.Ip, args.Port)];
		try
		{
			byte[] decompressedData = DataCompressor.DecompressData(args.Data);
			ICommand command = Serializer.FromBytes<ICommand>(decompressedData);
			command.PlayerId = playerId;

			lock (_commandQueueLock)
			{
				_commandQueue.Enqueue(command);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Invalid data received: {ex.Message}");
		}
	}

	private void OnConnectionEstablished(object? sender, DataEventArgs args)
	{
		string playerEndpoint = GetPlayerEndpoint(args.Ip, args.Port);
		uint playerId = (uint)_playerIds.Count;

		_playerIds[playerEndpoint] = playerId;
		_playerEndpoints[playerId] = playerEndpoint;
	}

	private void CalcStats(int deltaTime, float load, long elapsed)
	{
		#pragma warning disable CS0162
		if (!SHOW_STATS) return;
		#pragma warning restore CS0162

		_statInterval += (int)deltaTime;
		_statLoadSum += load;
		_statTicks++;

		if (_statInterval > STAT_INTERVAL_MS)
		{
			float loadAvg = (float)_statLoadSum/(float)_statTicks;

			float totalThroughput = (float)_statByteSum / ((float)_statInterval / 1000.0f);
			float totalThroughputKb = totalThroughput / 1024.0f;

			float userThroughput = totalThroughput / (float)System.Math.Max(_server.ConnectionCount, 1);
			float userThroughputKb = userThroughput / 1024.0f;

			float incomingThroughput = (float)_statByteIncSum / ((float)_statInterval / 1000.0f);
			float incomingThroughputKb = incomingThroughput / 1024.0f;

			float userIncomingThroughput = incomingThroughput / (float)System.Math.Max(_server.ConnectionCount, 1);
			float userIncomingThroughputKb = userIncomingThroughput / 1024.0f;

			float avgPacketSize = _statByteSum / System.MathF.Max(_statPacketsSent, 1);
			float avgIncPacketSize = (float)_statByteIncSum / System.MathF.Max(_statPacketsReceived, 1);

			// Console.Write($"[{elapsed:D8}] ");
			// Console.Write($"[{_totalTicks:D8}]  ");
			TimeSpan ts = TimeSpan.FromMilliseconds(elapsed);

			string stats = "";
			stats += $"[{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}]   ";

			stats += $"load: {loadAvg,5:F2}  | ";

			stats += $"send: {totalThroughputKb,5:F1} KB/s  | ";
			stats += $"recv: {incomingThroughputKb,5:F1} KB/s  | ";

			stats += $"pkt out: {(int)avgPacketSize,5} B  | ";
			stats += $"pkt in: {(int)avgIncPacketSize,5} B";

			Console.WriteLine(stats);


			if (loadAvg > 1.0f)
			{
				Console.WriteLine("Engine is overloaded!");
			}

			_statInterval = 0;
			_statLoadSum = 0.0f;
			_statTicks = 0;
			_statByteSum = 0;
			_statByteIncSum = 0;
			_statPacketsReceived = 0;
			_statPacketsSent = 0;
		}
	}
}
}
