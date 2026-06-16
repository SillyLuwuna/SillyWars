namespace RtsEngine
{

public class RtsEngine
{
	public bool HasStarted { get; private set; }
	private WorldState _state;

	public RtsEngine(WorldState state)
	{
		HasStarted = false;
		_state = state;
	}

	public void Start()
	{
		HasStarted = true;
	}

	public void Tick()
	{
		if (!HasStarted) throw new InvalidOperationException("cannot tick when the engine is not running.");
	}
}
}
