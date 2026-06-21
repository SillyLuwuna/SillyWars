using RtsEngine.Data;
using RtsEngine.Math;

namespace RtsEngine.Units
{

public class Knight : BaseUnit
{
	public Knight(Vec2 pos, uint ownerId) : base(pos, ownerId)
	{
		HP = 3;
		Range = 0.5f;
		AttackDamage = 1;
		AttackSpeed = 1.0f;
		MoveSpeed = 0.15f;
		Sight = 3;
		TrainCost = 10;
		TrainTime = 15.0f;
		Type = UnitType.Knight;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
	}

	public override void Tick()
	{
		base.Tick();
	}
}
}
