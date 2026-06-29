using static Tensorflow.Binding;
using static Tensorflow.KerasApi;
using Tensorflow;
using Tensorflow.Keras;
using Tensorflow.NumPy;
// using Tensorflow.Keras;
// using Tensorflow.Keras.Layers;
// using Tensorflow.Keras.Losses;
// using System;
// using System.Linq;
// using Tensorflow.Keras.Engine;
using RtsEngine.Commands;
using Tensorflow.Keras.Engine;
// using Tensorflow.Keras.ArgsDefinition;

namespace RtsEngine.AI
{

public class RtsAI : IRtsPlayer
{
	private static readonly int StateSize = RtsState.Size;
	private static readonly int ActionCount = Enum.GetValues(typeof(RtsAction)).Length;

	private IModel _model;
	private Random _rng;
	private const float _epsilon = 0.3f;
	// private float[] _lastStateInput;
	// private int _lastActionIndex;

	public RtsAI()
	{
		GenerateModel();
	}

	private void GenerateModel()
	{
		var inputs = keras.Input(shape: StateSize, name: "state");

		var x = keras.layers.Dense(16, activation: "relu").Apply(inputs);
		x = keras.layers.Dense(16, activation: "relu").Apply(x);
		var outputs = keras.layers.Dense(ActionCount, activation: "linear").Apply(x);

		_model = keras.Model(inputs, outputs, name: "dqn");

		_model.compile(
			optimizer: keras.optimizers.Adam(0.01f),
			loss: keras.losses.MeanSquaredError()
		);
	}

	private void Learn(RtsState state, RtsAction action, float reward)
	{
		NDArray stateArray = GetNumpyArray(state.Array);

		var targetQValues = Predict(stateArray);
		targetQValues[0, (int)action] = reward;

		_model.fit(stateArray, targetQValues, batch_size: 1, epochs: 1, verbose: 0);
	}

	private NDArray GetNumpyArray(float[] arr)
	{
		var ndArr = new NDArray(new float[1, arr.Length]);
		for (int i = 0; i < arr.Length; i++)
		{
			ndArr[0, i] = arr[i];
		}
		return ndArr;
	}

	public ICommand? MakePlay(WorldState state)
	{
		RtsState currState = new RtsState(state);

		RtsAction action;
		if (_rng.NextDouble() < _epsilon)
		{
			action = (RtsAction)_rng.Next(ActionCount);
		}
		else
		{
			action = PredictBestAction(currState);
		}

		return ActionToCommand(action);
	}

	private RtsAction PredictBestAction(RtsState state)
	{
		NDArray QValues = Predict(state);

		int max = 0;
		float maxVal = QValues[0, 0];
		for (int i = 1; i < ActionCount; i++)
		{
			if (QValues[0, i] > maxVal)
			{
				max = i;
				maxVal = QValues[0, i];
			}
		}

		return (RtsAction)max;
	}

	private NDArray Predict(RtsState state)
	{
		NDArray stateArray = GetNumpyArray(state.Array);
		return Predict(stateArray);
	}

	private NDArray Predict(NDArray stateArray)
	{
		return _model.predict(stateArray).numpy();
	}

	private ICommand ActionToCommand(RtsAction action)
	{

	}

	public void GameStarted(WorldState initialState)
	{

	}

	public void GameEnded(WorldState finalState)
	{

	}
}

}
