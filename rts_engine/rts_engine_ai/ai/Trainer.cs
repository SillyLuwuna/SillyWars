using System.Diagnostics;
using RtsEngine.Commands;
using RtsEngine.Data;

namespace RtsEngine.AI
{

public class Trainer
{
	private RtsEngine _engine = null!;
	private WorldState _initialState = null!;
	private WorldState _currState = null!;
	private List<IRtsPlayer> _players;

	private Stopwatch _statTickStopwatch;
	private Stopwatch _statGameStopwatch;
	private int _statTicks;
	private int _statTotalTicks;

	public Trainer(WorldState initialState)
	{
		_initialState = initialState;
		RestartState();
		RestartEngine();
		_players = new List<IRtsPlayer>();

		for (int i = 0; i < initialState.NumPlayers; i++)
		{
			_players.Add(new AIPlayer());
		}

		_statTickStopwatch = new Stopwatch();
		_statGameStopwatch = new Stopwatch();
	}

	private void RestartState()
	{
		byte[] stateData = Serializer.ToBytes(_initialState);
		_currState = Serializer.FromBytes<WorldState>(stateData);
	}

	private void RestartEngine()
	{
		_engine = RtsEngine.StartInstance(_currState, tps: 100);
	}

	public void RunGame()
	{
		StartGame();

		while (!_engine.State.IsGameOver)
		{
			MakeMoves();
			_engine.Tick();
			CalcStatistics();
		}

		EndGame();
	}

	private void StartGame()
	{
		RestartState();
		RestartEngine();

		_engine.Start(isServer: false, useInternalClock: false);

		foreach (IRtsPlayer player in _players)
		{
			player.GameStarted(_engine.State);
		}

		_statTickStopwatch.Restart();
		_statGameStopwatch.Restart();
		_statTicks = 0;
		_statTotalTicks = 0;
	}

	private void MakeMoves()
	{
		foreach (IRtsPlayer player in _players)
		{
			ICommand? play = player.MakePlay(_engine.State);
			if (play == null) continue;

			_engine.EnqueueCommand(play);
		}
	}

	private void EndGame()
	{
		_statTickStopwatch.Stop();
		_statGameStopwatch.Stop();

		foreach (IRtsPlayer player in _players)
		{
			player.GameEnded(_engine.State);
		}

		_engine.Stop();
	}

	private void CalcStatistics()
	{
		_statTicks++;
		_statTotalTicks++;

		long elapsed = _statTickStopwatch.ElapsedMilliseconds;
		if (elapsed >= 1000)
		{
			float tps = (float)_statTicks / ((float)elapsed / 1000f);

			_statTickStopwatch.Restart();
			_statTicks = 0;

			Console.WriteLine($"simulation speed: {tps,5} tps");
		}
	}

	public void RunGames(int numGames)
	{
		Console.WriteLine("Starting training...");
		for (int i = 0; i < numGames; i++)
		{
			RunGame();
			CalcGameStatistics(i);
		}
		Console.WriteLine("Training complete!");
	}

	private void CalcGameStatistics(int game)
	{
		float elapsedSeconds = (float)_statGameStopwatch.ElapsedMilliseconds / 1000f;

		Console.WriteLine($"=== Finished game {game,5:D} ===");
		Console.WriteLine($"elapsed: {elapsedSeconds,3:F3}");
		Console.WriteLine($"total ticks: {_statTotalTicks,3:F3}");
		Console.WriteLine($"============================");
	}
}

}
