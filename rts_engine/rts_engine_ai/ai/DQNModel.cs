using static Tensorflow.Binding;
using static Tensorflow.KerasApi;
using Tensorflow;
using Tensorflow.Keras;
using Tensorflow.NumPy;
using RtsEngine.Commands;
using Tensorflow.Keras.Engine;

namespace RtsEngine.AI
{

public class DQNModel
{
	private static readonly int StateSize = RtsState.Size;
	private static readonly int ActionCount = Enum.GetValues(typeof(RtsAction)).Length;

	private IModel _model;

	public DQNModel()
	{
		_model = GenerateModel();
	}

	public DQNModel(DQNModel copy) : this()
	{
		SetCopyFrom(copy);
	}

	public void SetCopyFrom(DQNModel copy)
	{
		_model.set_weights(copy._model.get_weights());
	}

	private IModel GenerateModel()
	{
		var inputs = keras.Input(shape: StateSize, name: "state");

		var x = keras.layers.Dense(16, activation: "relu").Apply(inputs);
		x = keras.layers.Dense(16, activation: "relu").Apply(x);
		var outputs = keras.layers.Dense(ActionCount, activation: "linear").Apply(x);

		IModel model = keras.Model(inputs, outputs, name: "dqn");

		model.compile(
			optimizer: keras.optimizers.Adam(0.01f),
			loss: keras.losses.MeanSquaredError()
		);

		return model;
	}

	public void Train(RtsState state, NDArray targetQValues)
	{
		_model.fit(GetNumpyArray(state.Array), targetQValues, batch_size: 1, epochs: 1, verbose: 0);
	}


	public void Learn(RtsState state, RtsAction action, RtsState resultingState, float reward)
	{
		NDArray stateArray = GetNumpyArray(state.Array);
		NDArray resultingStateArray = GetNumpyArray(resultingState.Array);

		RtsAction nextAction = PredictBestAction(resultingState, out float bestQValue);

		float targetQValue = reward + 0.95f * bestQValue;

		NDArray targetQValues = Predict(stateArray);
		targetQValues[0, (int)action] = targetQValue;

		_model.fit(stateArray, targetQValues, batch_size: 1, epochs: 1, verbose: 0);
	}

	public static NDArray GetNumpyArray(float[] arr)
	{
		var ndArr = new NDArray(new float[1, arr.Length]);
		for (int i = 0; i < arr.Length; i++)
		{
			ndArr[0, i] = arr[i];
		}
		return ndArr;
	}

	public float PredictBestQValue(RtsState state)
	{
		PredictBestAction(state, out float maxVal);
		return maxVal;
	}

	public RtsAction PredictBestAction(RtsState state)
	{
		return PredictBestAction(state, out _);
	}

	public RtsAction PredictBestAction(RtsState state, out float maxVal)
	{
		NDArray QValues = Predict(state);

		int max = 0;
		maxVal = QValues[0, 0];
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

	public NDArray Predict(RtsState state)
	{
		NDArray stateArray = GetNumpyArray(state.Array);
		return Predict(stateArray);
	}

	public NDArray Predict(NDArray stateArray)
	{
		return _model.predict(stateArray).numpy();
	}
}

}
