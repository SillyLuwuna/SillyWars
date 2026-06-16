namespace RtsEngine.Units;

public abstract class BaseUnit : Entity
{
	public int HP { get; protected set; }
	public float Range { get; protected set; }
	public int AttackDamage { get; protected set; }
	public float AttackSpeed { get; protected set; }
	public float MoveSpeed { get; protected set; }
	public int Sight { get; protected set; }
	public int TrainCost { get; protected set; }
	public float TrainTime { get; protected set; }
	public UnitType Type { get; protected set; }

	public UnitState State { get; protected set; }
	// Actions!

	public float Size { get; private set; } // for unit-unit collision

	private IReadOnlyList<BaseUnit> _units;

	public BaseUnit(IReadOnlyList<BaseUnit> units, Vec2 pos)
	{
		_units = units;
		Pos = pos;

		Size = 0.1f;
	}

	public override void Tick()
	{
		// abstract logic
		UnitTick();
	}

	public abstract void UnitTick();

	// public void Move(Grid<Cell> grid, Vec2 goal)
	// {
	// 	TODO
	// }
}
