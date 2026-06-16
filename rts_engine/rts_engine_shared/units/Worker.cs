using System.IO;

namespace RtsEngine.Units
{

public class Worker : BaseUnit, ISerializable
{
	public Worker(Vec2 pos) : base(pos)
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

	public override void SerializeFields(BinaryWriter writer)
	{
		base.SerializeFields(writer);
	}

	public override void DeserializeFields(BinaryReader reader)
	{
		base.DeserializeFields(reader);
	}

	public override void Tick()
	{
		base.Tick();
	}
}
}
