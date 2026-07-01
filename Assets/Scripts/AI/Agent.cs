#nullable enable

using RtsEngine;
using RtsEngine.AI;
using RtsEngine.Commands;
using Unity.InferenceEngine;
using UnityEngine;

public class Agent : MonoBehaviour
{
	public ModelAsset _modelAsset = null!;

	Unity.InferenceEngine.Worker _worker = null!;

	private RtsState? _lastState;
	private uint _playerId;
	private ulong _tick;
	private RtsActionUtils _actionUtils = null!;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

	public void Load(uint playerId)
	{
		_playerId = playerId;
		_actionUtils = new RtsActionUtils(_playerId);
		var model = ModelLoader.Load(_modelAsset);
		_worker = new Worker(model, BackendType.CPU);
		_tick = 0;
	}

	public float[] Predict(float[] state)
	{
		Tensor<float> input = new Tensor<float>(new TensorShape(1, state.Length), state);
		_worker.Schedule(input);

		Tensor<float> output = _worker.PeekOutput() as Tensor<float>;

		float[] actions = output!.DownloadToArray();

		// string qstate = "";
		// foreach (float f in actions)
		// {
		// 	qstate += $"{f} ";
		// }
		// Debug.Log(qstate);

		input.Dispose();
		output.Dispose();

		return actions;
	}

	public ICommand? MakePlay(WorldState state)
	{
		_actionUtils.Update(state);
		RtsState currState = new RtsState(_lastState, state, _playerId, _tick);

		// string stateArr = "";
		// foreach (float f in currState.Array)
		// {
		// 	stateArr += $"{f} ";
		// }
		// Debug.Log(stateArr);

		float[] qvalues = Predict(currState.Array);
		_lastState = currState;

		int max = 0;
		float maxVal = qvalues[0];
		for (int i = 1; i < qvalues.Length; i++)
		{
			if (qvalues[i] > maxVal)
			{
				max = i;
				maxVal = qvalues[i];
			}
		}

		RtsAction action = (RtsAction)max;
		// Debug.Log($"{action}");

		return _actionUtils.ActionToCommand(currState, state, action);
	}

	void OnDestroy()
	{
		_worker?.Dispose();
	}
}
