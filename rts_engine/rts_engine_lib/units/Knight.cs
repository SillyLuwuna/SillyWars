using System.Collections.Generic;
using System.IO;
using RtsEngine.EntityProperties;
using RtsEngine.Math;

namespace RtsEngine.Units
{

public class Knight : BaseUnit
{
	private static readonly HashSet<CommandType> ALLOWED_ACTIONS = new HashSet<CommandType>
	{
		CommandType.Move
	};

	public Knight(Vec2 pos, uint ownerId) : base(pos, ownerId)
	{
		HP = 3;
		Range = 0.5f;
		AttackDamage = 1;
		AttackSpeed = 1.0f;
		MoveSpeed = 0.10f;
		Sight = 3;
		TrainCost = 10;
		TrainTime = 15.0f;
		Type = UnitType.Knight;
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

	public override HashSet<CommandType> GetAllowedActions()
	{
		return ALLOWED_ACTIONS;
	}
}
}
