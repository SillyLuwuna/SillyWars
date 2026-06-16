using RtsEngine.Networking;

namespace RtsEngine
{

public class RtsEngine
{
	public const int TPS = 20;
	private const int INTERVAL_MS = 1000 / TPS;
	private const int STAT_INTERVAL_MS = 1000;
	private const int PORT = 13774;
	
	private const int NUM_PLAYERS = 1;

	public bool IsRunning { get; private set; }
	private WorldState _state;
	private Clock _clock;
	private Server _server;

	private int _statInterval;
	private float _statAvg;
	private int _statTicks;

	private Task? _currentBroadcastTask;
	private readonly object _broadcastLock;

	public RtsEngine(WorldState state)
	{
		_currentBroadcastTask = null;
		_broadcastLock = new object();
		IsRunning = false;
		_state = state;
		_clock = new Clock(INTERVAL_MS);
		_clock.Tick += TickSubscriber;
		_server = new Server(PORT, NUM_PLAYERS);
		Reset();
	}

	private void Reset()
	{
		_statInterval = 0;
		_statAvg = 0.0f;
		_statTicks = 0;
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

		lock (_broadcastLock)
		{
			if (_currentBroadcastTask != null && !_currentBroadcastTask.IsCompleted)
			{
				Console.WriteLine("Network bottleneck.");
				_currentBroadcastTask.Wait();
				// return;
			}
		}

		int numEntities = _state.Entities.Count;
		for (int i = 0; i < numEntities; i++)
		{
			_state.Entities[i].Tick();
		}

		_currentBroadcastTask = _server.BroadcastData(Serializer.ToBytes(_state));
	}

	private void TickSubscriber(object? sender, ClockEventArgs e)
	{
		_statInterval += (int)e.DeltaTime;
		_statAvg += e.Load;
		_statTicks++;
		if (_statInterval > STAT_INTERVAL_MS)
		{
			float loadAvg = (float)_statAvg/(float)_statTicks;
			Console.WriteLine($"[{e.ElapsedMs:D8}] Load avg: {loadAvg:F2}");
			if (loadAvg > 1.0f)
			{
				Console.WriteLine("Engine is overloaded!");
			}
			_statInterval = 0;
			_statAvg = 0.0f;
			_statTicks = 0;
		}
		try
		{
			Tick();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error during tick: {ex.Message}");
		}

	}
}
}
