using Tensorflow.NumPy;
using RtsEngine.Commands;

namespace RtsEngine.AI
{

public class RtsAI : IRtsPlayer
{
	private static readonly int StateSize = RtsState.Size;
	private static readonly int ActionCount = Enum.GetValues(typeof(RtsAction)).Length;

	private const float Epsilon = 0.3f;
	private const int GamesUntilNetworkMerge = 100;

	private int _currGames;
	private Random _rng;
	private DQNModel _policyNetwork;
	private DQNModel _targetNetwork;

	private RtsState? _lastState;
	private RtsAction _lastAction;

	public RtsAI()
	{
		_currGames = 0;
		_rng = new Random();
		_policyNetwork = new DQNModel();
		_targetNetwork = new DQNModel(_policyNetwork);
		_lastState = null;
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

	public ICommand? MakePlay(WorldState state, uint playerId)
	{
		RtsState currState = new RtsState(state, playerId);

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

		return RtsActionUtils.ActionToCommand(state, action);
	}

	public void GameStarted(WorldState initialState)
	{
		_lastState = null;
	}

	public void GameEnded(WorldState finalState)
	{
		_currGames++;
		if (_currGames > GamesUntilNetworkMerge)
		{
			_targetNetwork.SetCopyFrom(_policyNetwork);
			_currGames = 0;
		}

		// TODO reward for winning / losing
		// TODO check if game actually ended or if it was a draw;
	}

	public float CalcReward(RtsState lastState, RtsAction lastAction, RtsState currState)
	{

	}
}

}
