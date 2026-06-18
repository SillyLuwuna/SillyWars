using System.IO;
using RtsEngine.Data;
using RtsEngine.Math;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using System;

namespace RtsEngine.Units
{

public abstract class BaseUnit : Entity, ISerializable, IMovable
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
	protected UnitState _state;
	public UnitState State { get => _state; }

	public Map.Path? CurrPath { get; protected set; }
	public int CurrPathCheckpoint { get; protected set; }

	public float Size { get; protected set; } // for unit-unit collision

	public BaseUnit(Vec2 pos, uint ownerId) : base(ownerId)
	{
		Pos = pos;

		Size = 0.1f;
		CurrPath = null;
	}

	public override void Tick()
	{
		Move();
	}

	public override void SerializeFields(BinaryWriter writer)
	{
		base.SerializeFields(writer);
		writer.Write(HP);
		writer.Write(Range);
		writer.Write(AttackDamage);
		writer.Write(AttackSpeed);
		writer.Write(MoveSpeed);
		writer.Write(Sight);
		writer.Write(TrainCost);
		writer.Write(TrainTime);
		writer.Write((byte)Type);
		Serializer.Serialize(writer, _state);
		// State.SerializeFields(writer);
		writer.Write(Size);
	}

	public override void DeserializeFields(BinaryReader reader)
	{
		base.DeserializeFields(reader);
		HP = reader.ReadInt32();
		Range = reader.ReadSingle();
		AttackDamage = reader.ReadInt32();
		AttackSpeed = reader.ReadSingle();
		MoveSpeed = reader.ReadSingle();
		Sight = reader.ReadInt32();
		TrainCost = reader.ReadInt32();
		TrainTime = reader.ReadSingle();
		Type = (UnitType)reader.ReadByte();
		_state = Serializer.Deserialize<UnitState>(reader);
		Size = reader.ReadSingle();
	}

	public void Move(Grid<Cell> map, Vec2 goal)
	{
		// can be easily optimized by caching, and seeing when map changes to update cache
		PathFinding pathfinder = new PathFinding(map);
		PathOptimizer optimizer = new PathOptimizer(map);


		CurrPath = pathfinder.GetPath(Pos, goal);
		if (CurrPath.Count <= 1)
		{
			Halt();
			return;
		}

		CurrPath = optimizer.OptimizePath(pathfinder.GetPath(Pos, goal));
		CurrPathCheckpoint = 1;

		_state.IsWalking = true;
	}

	public void Move()
	{
		if (!_state.IsWalking) return;

		Vec2 target = CurrPath![CurrPathCheckpoint];

		if (target.Distance(Pos) <= MoveSpeed)
		{
			Pos = target;
			CurrPathCheckpoint++;
			if (CurrPathCheckpoint >= CurrPath.Count)
			{
				Halt();
			}
			return;
		}

		Vec2 direction = Pos.To(target).Unit;
		Pos += direction * MoveSpeed;
	}

	public void Halt()
	{
		_state.IsWalking = false;
		CurrPath = null;
	}
}
}
