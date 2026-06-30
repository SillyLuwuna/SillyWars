using Tensorflow.NumPy;
using RtsEngine.Commands;

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

	public RtsAI(DQNModel policyNetwork, DQNModel targetNetwork, uint playerId)
	{
		_policyNetwork = policyNetwork;
		_targetNetwork = targetNetwork;
		_playerId = playerId;
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

	public ICommand? MakePlay(WorldState state, ulong currTick)
	{
		_actionUtils.Update(state);
		RtsState currState = new RtsState(_lastState, state, _playerId, currTick);

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
			float reward = CalcReward(_lastState.Value, _lastAction, currState);
			Learn(_lastState.Value, _lastAction, currState, reward);
		}

		_lastState = currState;
		_lastAction = action;

		return _actionUtils.ActionToCommand(currState, state, action);
	}

	public void GameStarted(WorldState initialState)
	{
		_lastState = null;
		_actionUtils = new RtsActionUtils(_playerId);
	}

	public void GameEnded(WorldState finalState)
	{

		// TODO reward for winning / losing
		// TODO check if game actually ended or if it was a draw;
	}

	public float CalcReward(RtsState lastState, RtsAction lastAction, RtsState currState)
	{

	}
}

}
