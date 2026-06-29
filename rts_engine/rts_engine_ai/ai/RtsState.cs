namespace RtsEngine.AI
{

public struct RtsState
{
	public const int Size = 5;

	private float[] _state;

	public RtsState(WorldState worldState)
	{
		_state = new float[Size];
	}

	public float[] Array => _state;
}

}
