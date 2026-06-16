namespace RtsEngine.Units;

public class Worker : BaseUnit
{
	public Worker(IReadOnlyList<BaseUnit> units, Vec2 pos) : base(units, pos)
	{
		HP = 3;
		Range = 0.5f;
		AttackDamage = 1;
		AttackSpeed = 1.0f;
		MoveSpeed = 1.0f;
		Sight = 3;
		TrainCost = 10;
		TrainTime = 15.0f;
		Type = UnitType.Worker;
	}

	public override void UnitTick()
	{
	}
}
