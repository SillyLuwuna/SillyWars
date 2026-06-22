using System;
using RtsEngine.Data;
using RtsEngine.Math;

namespace RtsEngine.Units
{

public class Knight : BaseUnit
{
	public override int HitPoints { get; set; }
	public override int AttackDamage { get; set; }
	public override int AttackSpeed { get; set; }
	public override float AttackRange { get; set; }
	public override float ChaseDistance { get; set; }
	public override float AggroRange { get; set; }
	public override float MoveSpeed { get; set; }

	public Knight(Vec2 pos, uint ownerId) : base(pos, ownerId)
	{
		Radius = 0.2f;
		Mass = 1.0f;
		Friction = 1.0f;

		HitPoints = 5;
		AttackDamage = 2;
		AttackSpeed = 25;
		AttackRange = Radius + 0.1f;
		ChaseDistance = 3.0f;
		AggroRange = 3.0f;
		MoveSpeed = 0.15f;
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
