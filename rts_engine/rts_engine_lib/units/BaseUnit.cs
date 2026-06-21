using System.IO;
using RtsEngine.Data;
using RtsEngine.Math;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using System;
using RtsEngine.Physics;

namespace RtsEngine.Units
{

public abstract class BaseUnit : PhysicsObject, ISerializable, IMovable
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

	public BaseUnit(Vec2 pos, uint ownerId) : base(pos, ownerId, 1.0f, 0.1f, 0.2f)
	{
		Radius = 0.3f;
		Mass = 1.0f;
		Friction = 1.0f;

		CurrPath = null;
	}

	public override void Tick()
	{
		Move();
	}

	public override void SerializeFields(SerializerWriter writer)
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
		writer.Write(Type);
		writer.Write(_state);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
		HP = reader.Read<int>();
		Range = reader.Read<float>();
		AttackDamage = reader.Read<int>();
		AttackSpeed = reader.Read<float>();
		MoveSpeed = reader.Read<float>();
		Sight = reader.Read<int>();
		TrainCost = reader.Read<int>();
		TrainTime = reader.Read<float>();
		Type = reader.Read<UnitType>();
		_state = reader.Read<UnitState>();
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
		this.ApplyForce(direction * MoveSpeed);
		// Pos += direction * MoveSpeed;
	}

	public void Halt()
	{
		ClearVelocity();
		_state.IsWalking = false;
		CurrPath = null;
	}
}
}
