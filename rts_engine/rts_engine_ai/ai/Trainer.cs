using static Tensorflow.Binding;
using Tensorflow;
using RtsEngine.Commands;
using System.Diagnostics;
using RtsEngine.Data;
using RtsEngine.Resources;

namespace RtsEngine.AI
{

public class Trainer
{
	private const int ExpectedTps = 100;
	public const ulong MaxAllowedTicks = ExpectedTps * 60;

	private const int GamesUntilNetworkMerge = 10;

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

	private DateTime _now = DateTime.Now;

	public Trainer(WorldState initialState, string network = "")
	{
		_initialState = initialState;
		RestartState();
		RestartEngine();
		_players = new List<IRtsPlayer>();

		if (network == "")
		{
			_policyNetwork = new DQNModel();
		}
		else
		{
			_policyNetwork = DQNModel.Load(network);
		}
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
		var oldOut = Console.Out;
		try
		{
			Console.SetOut(TextWriter.Null);
			if (_engine != null)
			{
				_engine.Stop();
			}

			_engine = new RtsEngine(_currState);
			_engine.Start(isServer: false, useInternalClock: false);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.Message);
			Console.Error.WriteLine(ex.StackTrace);
		}
		finally
		{
			Console.SetOut(oldOut);
		}
	}

	public void RunGame()
	{
		StartGame();

		while (!_engine.State.IsGameOver && ((ulong)_statTotalTicks < MaxAllowedTicks))
		{
			int player0Gold = _currState.GetResource(0, Resource.Gold);
			int player1Gold = _currState.GetResource(1, Resource.Gold);
			int enqueuedUnits0 = _currState._playerTotalEnqueuedUnits[0];
			int enqueuedUnits1 = _currState._playerTotalEnqueuedUnits[1];
			// Console.WriteLine($"{player0Gold}/{player1Gold} | {_currState.Units.Count}");
			if (_currState.Units.Count <= 0 && player0Gold < 20 && player1Gold < 20 && enqueuedUnits0 <= 0 && enqueuedUnits1 <= 0) break;
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

			ICommand? play = player.MakePlay(_engine.State, _statTotalTicks);
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
			player.GameEnded(_engine.State, _statTotalTicks);
		}

		var oldOut = Console.Out;
		try
		{
			Console.SetOut(TextWriter.Null);
			_engine.Stop();
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.Message);
			Console.Error.WriteLine(ex.StackTrace);
		}
		finally
		{
			Console.SetOut(oldOut);
		}
	}

	private void CalcStatistics()
	{
		_statTicks++;
		_statTotalTicks++;

		// long elapsed = _statTickStopwatch.ElapsedMilliseconds;
		// if (elapsed >= 10000)
		// {
		// 	float tps = (float)_statTicks / ((float)elapsed / 1000f);
		//
		// 	_statTickStopwatch.Restart();
		// 	_statTicks = 0;
		//
		// 	// Console.WriteLine($"simulation speed: {tps,5} tps");
		// }
	}

	public void RunGames(int numGames)
	{
		Console.WriteLine("Starting training...");
		for (int i = 0; i < numGames; i++)
		{
			RunGame();
			SaveNetworks(i);
			CalcGameStatistics(i);
			if ((i + 1) % GamesUntilNetworkMerge == 0)
			{
				Console.WriteLine("Merging...");
				MergeNetworks();
			}
		}
		Console.WriteLine("Training complete!");
	}

	private void MergeNetworks()
	{
		_targetNetwork.SetCopyFrom(_policyNetwork);
	}
	
	private void SaveNetworks(int game)
	{
		_policyNetwork.Save($"networks/policy_{_now.Hour:00}{_now.Minute:00}{_now.Second:00}_{game}.h5");
		_policyNetwork.SaveFull($"networks/policy_{_now.Hour:00}{_now.Minute:00}{_now.Second:00}_{game}_full.h5");
	}

	private void CalcGameStatistics(int game)
	{
		float elapsedSeconds = (float)_statGameStopwatch.ElapsedMilliseconds / 1000f;
		float tps = (float)_statTotalTicks / elapsedSeconds;
		bool p0Won = _currState.PlayerWon(0);
		bool p1Won = _currState.PlayerWon(1);
		bool draw = p0Won == p1Won;
		int win = -1;
		if (draw)
		{
			win = -1;
		}
		else if (p0Won)
		{
			win = 0;
		}
		else if (p1Won)
		{
			win = 1;
		}

		Console.WriteLine($"======== Finished game {game,4:D} ========");
		Console.WriteLine($"elapsed:\t\t{elapsedSeconds:F1} s");
		Console.WriteLine($"total ticks:\t\t{_statTotalTicks:D}");
		Console.WriteLine($"simulation speed:\t{tps:F0} TPS");
		Console.WriteLine($"won: {win}");
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
