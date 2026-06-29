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

	public int TPS { get; private set; }
	private const int Port = 13774;

	private const int StatIntervalMs = 1000;
	private const bool ShowStats = true;
	private const bool DEBUG = true;
	
	private object _tickLock;

	public bool IsRunning { get; private set; }
	private PhysicsEngine _physicsEngine;
	private WorldState _state;
	private Clock _clock;
	private Server _server;
	private Dictionary<string, uint> _playerIds;
	private Dictionary<uint, string> _playerEndpoints;

	private object _commandQueueLock;
	private Queue<ICommand> _commandQueue;

	private Random _rng;

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

	private bool _isServer;
	private bool _useInternalClock;

	public event EventHandler<WorldState>? TickEnded;

	public static RtsEngine StartInstance(WorldState state, int tps = 20)
	{
		if (_instance != null)
		{
			_instance.Stop();
		}

		_instance = new RtsEngine(state, tps);
		return _instance;
	}

	public static RtsEngine Instance
	{
		get => _instance!;
	}

	private RtsEngine(WorldState state, int tps)
	{
		TPS = tps;
		_tickLock = new object();
		_rng = new Random();
		_physicsEngine = new PhysicsEngine();
		_currentBroadcastTask = null;
		_broadcastLock = new object();
		_playerIds = new Dictionary<string, uint>();
		_playerEndpoints = new Dictionary<uint, string>();
		_commandQueue = new Queue<ICommand>();
		_commandQueueLock = new object();
		IsRunning = false;
		_state = state;
		_clock = new Clock(1000 / tps);
		_clock.Tick += TickSubscriber;
		_server = new Server(Port, _state.NumPlayers);
		_server.MessageReceived += OnDataReceived;
		_server.ConnectionEstablished += OnConnectionEstablished;
		_server.ConnectionLost += OnConnectionLost;
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
		_isServer = false;
		_useInternalClock = false;

		lock(_commandQueueLock)
		{
			_commandQueue.Clear();
		}
	}

	public async Task Start(bool isServer = true, bool useInternalClock = true)
	{
		Console.WriteLine("Starting engine...");
		IsRunning = true;
		_isServer = isServer;
		_useInternalClock = useInternalClock;
		if (_isServer)
		{
			_ = _server.StartAsync();
		}
		if (_useInternalClock)
		{
			_clock.Start();
		}
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

		lock (_tickLock)
		{
			_totalTicks++;

			WaitForPreviousTickBroadcast();
			CleanupDestroyedEntities();
			ExecutePlayerCommands();
			UpdateWorldState();
			UpdatePhysics();
			CheckWinCondition();
			if (_isServer)
			{
				_currentBroadcastTask = BroadcastWorldState();
			}
			OnTickEnded();
		}
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

	private void CheckWinCondition()
	{
		_state.CheckWinCondition();
	}

	private async Task BroadcastWorldState()
	{
		foreach (string endpoint in _playerIds.Keys)
		{
			await SendWorldState(endpoint);
		}
	}

	private async Task SendWorldState(string endpoint)
	{
		_state.SetPlayerVersion((int)_playerIds[endpoint]);
		byte[] data = Serializer.ToBytes(_state);
		byte[] compressedData = DataCompressor.CompressData(data);

		await _server.SendData(compressedData, endpoint);

		_statByteSum += compressedData.Length;
		_statPacketsSent++;
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
			#pragma warning disable CS0162
			if (DEBUG) Console.WriteLine(ex.StackTrace);
			#pragma warning restore CS0162
		}

	}

	private void OnDataReceived(object? sender, DataEventArgs args)
	{
		_statByteIncSum += args.Data.Length;
		_statPacketsReceived++;

		uint playerId = _playerIds[CustomTcpClient.GenerateEndpoint(args.Ip, args.Port)];
		try
		{
			byte[] decompressedData = DataCompressor.DecompressData(args.Data);
			ICommand command = Serializer.FromBytes<ICommand>(decompressedData);
			command.PlayerId = playerId;
			EnqueueCommand(command);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Invalid data received: {ex.Message}");
		}
	}

	public void EnqueueCommand(ICommand command)
	{
		if (_state.PlayerLost(command.PlayerId)) return;

		lock (_commandQueueLock)
		{
			_commandQueue.Enqueue(command);
		}
	}

	private void OnConnectionEstablished(object? sender, DataEventArgs args)
	{
		string playerEndpoint = CustomTcpClient.GenerateEndpoint(args.Ip, args.Port);
		uint playerId = (uint)_playerIds.Count;

		_playerIds[playerEndpoint] = playerId;
		_playerEndpoints[playerId] = playerEndpoint;
	}

	private void OnConnectionLost(object? sender, DataEventArgs args)
	{
		string playerEndpoint = CustomTcpClient.GenerateEndpoint(args.Ip, args.Port);

		uint playerId = _playerIds[playerEndpoint];

		_playerIds.Remove(playerEndpoint);
		_playerEndpoints.Remove(playerId);
	}

	private void CalcStats(int deltaTime, float load, long elapsed)
	{
		#pragma warning disable CS0162
		if (!ShowStats) return;
		#pragma warning restore CS0162

		_statInterval += (int)deltaTime;
		_statLoadSum += load;
		_statTicks++;

		if (_statInterval > StatIntervalMs)
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

	// random value between 0 and 1;
	public double Rng => _rng.NextDouble();

	// random integer between 0 and max (inclusive);
	public int RngInterval(int max)
	{
		return _rng.Next() % (max + 1);
	}

	private void OnTickEnded()
	{
		TickEnded?.Invoke(this, _state);
	}
}
}
