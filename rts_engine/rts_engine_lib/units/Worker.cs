using RtsEngine.Data;
using RtsEngine.Math;

namespace RtsEngine.Units
{

public class Worker : BaseUnit
{
	public override int HitPoints { get; set; }
	public override int AttackDamage { get; set; }
	public override int AttackSpeed { get; set; }
	public override float AttackRange { get; set; }
	public override float ChaseDistance { get; set; }
	public override float AggroRange { get; set; }
	public override float MoveSpeed { get; set; }

	public Worker(Vec2 pos, uint ownerId) : base(pos, ownerId)
	{
		HitPoints = 1;
		AttackDamage = 1;
		AttackSpeed = 15;
		AttackRange = 0.25f;
		ChaseDistance = 2.0f;
		AggroRange = 2.0f;
		MoveSpeed = 0.18f;

		Radius = 0.2f;
		Mass = 1.0f;
		Friction = 1.0f;
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
