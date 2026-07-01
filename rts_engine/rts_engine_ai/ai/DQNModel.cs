using static Tensorflow.Binding;
using static Tensorflow.KerasApi;
using Tensorflow;
using Tensorflow.Keras;
using Tensorflow.NumPy;
using RtsEngine.Commands;
using Tensorflow.Keras.Engine;
using System.Diagnostics;
using System.Reflection;
using Tensorflow.Keras.Engine.DataAdapters;
using Tensorflow.Keras.ArgsDefinition;

namespace RtsEngine.AI
{

public class DQNModel
{
	private static MethodInfo _predictStepMethod = typeof(Model).GetMethod("predict_step", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance)!;
	private static readonly MethodInfo _trainStepMethod = typeof(Model)
    .GetMethod("train_step", 
        BindingFlags.InvokeMethod | 
        BindingFlags.NonPublic | 
        BindingFlags.Instance,
        null,
        new Type[] { typeof(DataHandler), typeof(Tensors), typeof(Tensors) },
        null)!;int StateSize = RtsState.Size;

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

		var x = keras.layers.Dense(64, activation: "relu").Apply(inputs);
		x = keras.layers.Dense(32, activation: "relu").Apply(x);
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
		var oldOut = Console.Out;
		// var sw = Stopwatch.StartNew();
		try
		{
			// Console.SetOut(TextWriter.Null);
			Tensors x = tf.convert_to_tensor(GetNumpyArray(state.Array));
			Tensors y = tf.convert_to_tensor(targetQValues);

			DataHandlerArgs args = new DataHandlerArgs();
			args.BatchSize = 1;
			args.Model = _model;
			args.X = x;
			args.Y = y;
			// args.Workers = 8;
			// args.UseMultiprocessing = true;
			// args.StepsPerEpoch = 1;

			DataHandler dataHandler = new DataHandler(args);

			_ = _trainStepMethod.Invoke(_model, new object[] { dataHandler, x, y })!;
			// var result = (Dictionary<string, float>)_trainStepMethod.Invoke(_model, new object[] { dataHandler, x, y })!;

			// Console.WriteLine(result["loss"]);

			// _model.fit(GetNumpyArray(state.Array), targetQValues, batch_size: 1, epochs: 1, verbose: 0);
		}
		finally
		{
			Console.SetOut(oldOut);
		}
		// sw.Stop();
		// Console.WriteLine($"Train: {sw.ElapsedMilliseconds} ms");
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
			// Console.Write($"{QValues[0, i]} ");
			if (QValues[0, i] > maxVal)
			{
				max = i;
				maxVal = QValues[0, i];
			}
		}
		// Console.WriteLine();

		return (RtsAction)max;
	}

	public NDArray Predict(RtsState state)
	{
		NDArray stateArray = GetNumpyArray(state.Array);
		return Predict(stateArray);
	}

	public NDArray Predict(NDArray stateArray)
	{
		// var sw = Stopwatch.StartNew();

		// Tensor tensor = new Tensor(stateArray);
		// Tensor tensors = new Tensors(tensor);
		Tensors tensors = tf.convert_to_tensor(stateArray);
		Tensors prediction = (Tensors)_predictStepMethod.Invoke(_model, new object[] { tensors })!;
		// Tensors prediction2 = _model.predict(stateArray, verbose: 0);
		// Tensors tensor = tf.constant(prediction);
		// sw.Stop();
		// Console.WriteLine($"prediction: {sw.ElapsedMilliseconds} ms");
		return prediction.numpy();
	}

	public void Save(string path)
	{
		_model.save_weights(path);
	}

	public void SaveFull(string path)
	{
		_model.save(path);
	}

	public static DQNModel Load(string path)
	{
		DQNModel model = new DQNModel();
		model._model.load_weights(path);
		return model;
	}
}

}
