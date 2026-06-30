using Tensorflow.NumPy;
using RtsEngine.Commands;
using RtsEngine.Math;

namespace RtsEngine.AI
{

public class RtsAI : IRtsPlayer
{
	private static readonly int StateSize = RtsState.Size;
	private static readonly int ActionCount = Enum.GetValues(typeof(RtsAction)).Length;

	private const float Epsilon = 0.3f;

	private int _currGames;
	private Random _rng;
	private DQNModel _policyNetwork;
	private DQNModel _targetNetwork;

	private RtsActionUtils _actionUtils;

	private RtsState? _lastState;
	private RtsAction _lastAction;

	private uint _playerId;
	private uint _enemyId;

	public RtsAI(DQNModel policyNetwork, DQNModel targetNetwork, uint playerId)
	{
		_policyNetwork = policyNetwork;
		_targetNetwork = targetNetwork;
		_playerId = playerId;
		_enemyId = playerId == 0 ? 1u : 0u;
		_currGames = 0;
		_rng = new Random();
		_lastState = null;
		_actionUtils = new RtsActionUtils(_playerId);
	}

	private void Learn(RtsState state, RtsAction actionTaken, RtsState resultingState, float reward)
	{
		RtsAction nextAction = _policyNetwork.PredictBestAction(resultingState);
		float bestQValue = _targetNetwork.PredictBestQValue(resultingState);
		float targetQValue = reward + 0.95f * bestQValue;

		NDArray targetQValues = _policyNetwork.Predict(state);
		targetQValues[0, (int)actionTaken] = targetQValue;

		_policyNetwork.Train(state, targetQValues);
	}

	public ICommand? MakePlay(WorldState currWorldState, ulong currTick)
	{
		_actionUtils.Update(currWorldState);
		RtsState currState = new RtsState(_lastState, currWorldState, _playerId, currTick);

		RtsAction action;
		if (_rng.NextDouble() < Epsilon)
		{
			action = (RtsAction)_rng.Next(ActionCount);
		}
		else
		{
			action = _policyNetwork.PredictBestAction(currState);
		}

		if (_lastState != null)
		{
			float reward = CalcReward(_lastState.Value, _lastAction, currState, currWorldState);
			Learn(_lastState.Value, _lastAction, currState, reward);
		}

		_lastState = currState;
		_lastAction = action;

		return _actionUtils.ActionToCommand(currState, currWorldState, action);
	}

	public void GameStarted(WorldState initialState)
	{
		_lastState = null;
		_actionUtils = new RtsActionUtils(_playerId);
	}

	public void GameEnded(WorldState finalState, ulong currTick)
	{
		RtsState currState = new RtsState(_lastState, finalState, _playerId, currTick);
		float reward = CalcReward(_lastState!.Value, _lastAction, currState, finalState);
		Learn(_lastState.Value, _lastAction, currState, reward);
	}

	public float CalcReward(RtsState lastState, RtsAction lastAction, RtsState currState, WorldState currWorldState)
	{
		float reward = 0;

		// economy rewards
		reward += currState.GetValue(StateEntry.GoldIncome) * 0.01f;

		// units loss/made reward
		reward += (currState.GetValue(StateEntry.Workers) - lastState.GetValue(StateEntry.Workers)) * 0.3f;
		reward += (currState.GetValue(StateEntry.Knights) - lastState.GetValue(StateEntry.Knights)) * 0.2f;

		// units killed reward
		reward += MathF.Min(-(currState.GetValue(StateEntry.EnemyWorkers) - lastState.GetValue(StateEntry.EnemyWorkers)) * 2.0f, 0);
		reward += MathF.Min(-(currState.GetValue(StateEntry.EnemyKnights) - lastState.GetValue(StateEntry.EnemyKnights)) * 2.5f, 0);

		// structures lost/built reward
		float lastBarracks = lastState.GetValue(StateEntry.Barracks);
		float currBarracks = currState.GetValue(StateEntry.Barracks);
		float lastCastles = lastState.GetValue(StateEntry.Castles);
		float currCastles = currState.GetValue(StateEntry.Castles);

		if (lastBarracks <= currBarracks)
		{
			// barracks built
			if (F.Zero(lastBarracks))
			{
				reward += 5f;
			}
			else
			{
				reward += (currBarracks - lastBarracks) * 0.1f;
			}
		}
		else
		{
			// barracks lost
			reward += (currBarracks - lastBarracks) * 2f;
		}

		if (lastCastles <= currCastles)
		{
			// castles built
			if (F.Zero(lastCastles))
			{
				reward += 10f;
			}
			else
			{
				reward += (currCastles - lastCastles) * 0.1f;
			}
		}
		else
		{
			// castles lost
			reward += (currCastles - lastCastles) * 5f;
		}

		// structures destroyed
		reward += MathF.Min(-(currState.GetValue(StateEntry.EnemyBarracks) - lastState.GetValue(StateEntry.EnemyBarracks)) * 5f, 0);
		reward += MathF.Min(-(currState.GetValue(StateEntry.EnemyCastles) - lastState.GetValue(StateEntry.EnemyCastles)) * 10f, 0);

		// idle workers
		reward += -currState.GetValue(StateEntry.IdleWorkers) * 0.5f;

		// hoarding gold
		if (currState.GetValue(StateEntry.Gold) > 300f && currState.GetValue(StateEntry.TotalUnits) <= 49f)
		{
			reward += - (currState.GetValue(StateEntry.Gold) - 300f) * 0.01f;
		}

		// game over rewards
		if (currWorldState.IsGameOver)
		{
			if (IsTie(currWorldState)) reward += -0.5f;
			else if (currWorldState.PlayerWon(_playerId)) reward += 100f;
			else reward += -100f;
		}

		return reward;
	}

	private bool IsTie(WorldState state)
	{
		return (state.PlayerWon(_playerId) == state.PlayerWon(_enemyId));
	}
}

}
