using System;
using System.Threading;
using System.Threading.Tasks;

namespace RtsEngine
{

public class ClockEventArgs : EventArgs
{
	public long ElapsedMs { get; private set; }
	public int DeltaTime { get; private set; }
	public float Load { get; private set; }

	public ClockEventArgs(long elapsedMilliseconds, int deltaTime, float load)
	{
		ElapsedMs = elapsedMilliseconds;
		DeltaTime = deltaTime;
		Load = load;
	}
}

public class Clock
{
	private int _msPerTick;
	private CancellationTokenSource? _cancellationTokenSource;
	private Task? _clockTask;
	private DateTime _startTime;
	private long _totalElapsedMs;
	private int _deltaTime;
	private float _load;

	public event EventHandler<ClockEventArgs>? Tick;
	public bool IsRunning { get; private set; }

	public Clock(int msPerTick)
	{
		_msPerTick = msPerTick;

		_cancellationTokenSource = null;
		_clockTask = null;
		_startTime = DateTime.Now;
		_totalElapsedMs = 0;
		_deltaTime = 0;
		_load = 0.0f;

		IsRunning = false;
	}

	public void Start()
	{
		if (IsRunning) return;

		IsRunning = true;
		_cancellationTokenSource = new CancellationTokenSource();
		_startTime = DateTime.Now;
		_totalElapsedMs = 0;
		_deltaTime = 0;
		_load = 0.0f;

		_clockTask = RunClockAsync(_cancellationTokenSource.Token);
	}

	public async Task StopAsync()
	{
		if (!IsRunning) return;

		IsRunning = false;
		_cancellationTokenSource?.Cancel();

		if (_clockTask != null)
		{
			try
			{
				await _clockTask;
			}
			catch (OperationCanceledException) {}
		}

		_cancellationTokenSource?.Dispose();
		_cancellationTokenSource = null;
	}

	private async Task RunClockAsync(CancellationToken cancellationToken)
	{
		try
		{
			long lastTicks = DateTime.Now.Ticks;

			while (!cancellationToken.IsCancellationRequested)
			{
				int delay = _msPerTick;

				await Task.Delay(delay, cancellationToken);

				long currTicks = DateTime.Now.Ticks;

				_deltaTime = (int)((currTicks - lastTicks) / TimeSpan.TicksPerMillisecond);
				_totalElapsedMs += _deltaTime;
				// _load = (float)(_msPerTick * TimeSpan.TicksPerMillisecond) / (float)(currTicks - lastTicks);

				OnTick(new ClockEventArgs(_totalElapsedMs, _deltaTime, _load));

				long ticksAfter = DateTime.Now.Ticks;
				_load = (ticksAfter - currTicks) / (float)(_msPerTick * TimeSpan.TicksPerMillisecond);

				lastTicks = currTicks;

			}
		}
		catch (OperationCanceledException) { }
	}

	protected virtual void OnTick(ClockEventArgs e)
	{
		Tick?.Invoke(this, e);
	}
}

}
