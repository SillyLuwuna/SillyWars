using System;
using RtsEngine.Data;
using RtsEngine.Math;
using RtsEngine.Resources;

namespace RtsEngine.Units
{

public class Knight : BaseUnit
{
	public const float BaseRadius = 0.2f;
	public const float BaseMass = 1.0f;
	public const float BaseFriction = 1.0f;

	public const int BaseHitPoints = 10;
	public const int BaseAttackDamage = 3;
	public const int BaseAttackSpeed = 35;
	public const float BaseAttackRange = BaseRadius + 0.1f;
	public const float BaseChaseDistance = 3.0f;
	public const float BaseAggroRange = 3.0f;
	public const float BaseMoveSpeed = 0.11f;

	public const int BaseProductionTime = 10 * 10;

	public override int HitPoints { get; set; }
	public override int AttackDamage { get; set; }
	public override int AttackSpeed { get; set; }
	public override float AttackRange { get; set; }
	public override float ChaseDistance { get; set; }
	public override float AggroRange { get; set; }
	public override float MoveSpeed { get; set; }
	public override int ProductionTime { get; set; }

	public override ResourceStack Cost { get => new ResourceStack(Resource.Gold, 10); }

	public Knight(Vec2 pos, uint ownerId) : base(pos, ownerId, BaseMass, BaseRadius, BaseFriction)
	{
		HitPoints = BaseHitPoints;
		AttackDamage = BaseAttackDamage;
		AttackSpeed = BaseAttackSpeed;
		AttackRange = BaseAttackRange;
		ChaseDistance = BaseChaseDistance;
		AggroRange = BaseAggroRange;
		MoveSpeed = BaseMoveSpeed;
		ProductionTime = BaseProductionTime;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
	}

	public override UnitType UnitType { get => UnitType.Knight; }

	public override void Tick()
	{
		base.Tick();
	}
}
}
