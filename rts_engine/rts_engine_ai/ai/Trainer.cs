using System.Diagnostics;
using RtsEngine.Commands;
using RtsEngine.Data;
using RtsEngine.Map;
using RtsEngine.Math;
using RtsEngine.Structures;

namespace RtsEngine.AI
{

public class Trainer
{
	private const int ExpectedTps = 500;
	public const ulong MaxAllowedTicks = ExpectedTps * 60;

	private const int GamesUntilNetworkMerge = 100;

	private RtsEngine _engine = null!;
	private WorldState _initialState = null!;
	private WorldState _currState = null!;
	private List<IRtsPlayer> _players;

	private Stopwatch _statTickStopwatch;
	private Stopwatch _statGameStopwatch;
	private ulong _statTicks;
	private ulong _statTotalTicks;

	private DQNModel _policyNetwork;
	private DQNModel _targetNetwork;

	// HashSet<Vec2Int> _validBuildLocations;

	public Trainer(WorldState initialState)
	{
		_initialState = initialState;
		// _validBuildLocations = GetValidBuildLocations(initialState);
		RestartState();
		RestartEngine();
		_players = new List<IRtsPlayer>();

		_policyNetwork = new DQNModel();
		_targetNetwork = new DQNModel(_policyNetwork);


		for (uint i = 0; i < initialState.NumPlayers; i++)
		{
			_players.Add(new RtsAI(_policyNetwork, _targetNetwork, i));
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
		if (_engine != null)
		{
			_engine.Stop();
		}

		_engine = new RtsEngine(_currState);
	}

	public void RunGame()
	{
		StartGame();

		while (!_engine.State.IsGameOver && ((ulong)_statTicks < MaxAllowedTicks))
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
		for (int i = 0; i < _players.Count; i++)
		{
			IRtsPlayer player = _players[i];

			ICommand? play = player.MakePlay(_engine.State, _statTicks);
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
			player.GameEnded(_engine.State, _statTicks);
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
			if (i % GamesUntilNetworkMerge == 0)
			{
				MergeNetworks();
			}
			RunGame();
			CalcGameStatistics(i);
		}
		Console.WriteLine("Training complete!");
	}

	private void MergeNetworks()
	{
		_targetNetwork.SetCopyFrom(_policyNetwork);
	}

	private void CalcGameStatistics(int game)
	{
		float elapsedSeconds = (float)_statGameStopwatch.ElapsedMilliseconds / 1000f;

		Console.WriteLine($"=== Finished game {game,5:D} ===");
		Console.WriteLine($"elapsed: {elapsedSeconds,3:F3}");
		Console.WriteLine($"total ticks: {_statTotalTicks,3:F3}");
		Console.WriteLine($"============================");
	}

	// private static HashSet<Vec2Int> GetValidBuildLocations(WorldState state, StructureType type)
	// {
	// 	HashSet<Vec2Int> validBuildLocations = new HashSet<Vec2Int>();
	//
	// 	Grid<Cell> map = state.Map;
	// 	Vec2Int currPos = Vec2Int.Zero;
	// 	while (currPos.x < map.Width)
	// 	{
	// 		while (currPos.y < map.Height)
	// 		{
	// 			BaseStructure structure = BaseStructure.FromType(type, state, ~0u, currPos);
	// 			if (!structure.IsAreaObstructed)
	// 			{
	// 				validBuildLocations.Add(currPos);
	// 			}
	// 		}
	// 	}
	//
	// 	return validBuildLocations;
	// }
}

}
