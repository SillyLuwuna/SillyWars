namespace RtsEngine;

public class RtsEngine
{
	public bool HasStarted { get; private set; }
	private WorldState _state;
	private Grid<Cell> _grid;

	public RtsEngine(WorldState state)
	{
		HasStarted = false;
		_state = state;
		// _grid = map;
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
