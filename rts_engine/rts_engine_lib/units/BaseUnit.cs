using System.IO;
using RtsEngine.Data;
using RtsEngine.Math;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using System;
using RtsEngine.Physics;
using System.Collections.Generic;

namespace RtsEngine.Units
{

public abstract class BaseUnit : PhysicsObject, ISerializable, IMovable, IAttacker, IDestroyable
{
	public bool IsDestroyed { get; set; }

	public abstract int HitPoints { get; set; }

	public abstract int AttackDamage { get; set; }
	public abstract int AttackSpeed { get; set; }
	public abstract float AttackRange { get; set; }
	public abstract float ChaseDistance { get; set; }

	private BaseUnit? _target;
	private bool _isDirectTarget;
	private int _cooldown;
	private Vec2 _pivot;
	private bool _isGoingToPivot;
	private Vec2? _walkGoal;
	private bool _isChasingTarget;
	private int _targetedByAmount;

	public abstract float MoveSpeed { get; set; }
	public Map.Path? CurrPath { get; set; }
	public int CurrPathCheckpoint { get; set; }

	private EntityState _state;
	public EntityState State { get => _state; set => _state = value; }

	public BaseUnit(Vec2 pos, uint ownerId, float mass=1.0f, float radius=0.2f, float friction=1.0f) : base(pos, ownerId, mass, radius, friction)
	{
		IsDestroyed = false;
		_target = null;
		_isDirectTarget = false;
		_cooldown = 0;
		_walkGoal = null;
		_isGoingToPivot = false;
		_isChasingTarget = false;
		_targetedByAmount = 0;

		CurrPath = null;
		_state = new EntityState();
	}

	public override void Tick()
	{
		base.Tick();

		MoveTick();
		AttackTick();
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
		writer.Write(HitPoints);
		writer.Write(AttackDamage);
		writer.Write(AttackSpeed);
		writer.Write(AttackRange);
		writer.Write(ChaseDistance);
		writer.Write(MoveSpeed);
		writer.Write(State);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
		HitPoints = reader.Read<int>();
		AttackDamage = reader.Read<int>();
		AttackSpeed = reader.Read<int>();
		AttackRange = reader.Read<float>();
		ChaseDistance = reader.Read<float>();
		MoveSpeed = reader.Read<float>();
		State = reader.Read<EntityState>();
	}

	public void SetGoal(Grid<Cell> map, Vec2 goal)
	{
		_walkGoal = goal;
		SetPathfinding(goal);
	}

	private void SetPathfinding(Vec2 goal)
	{
		Grid<Cell> map = RtsEngine.Instance.State.Map;
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

	public void MoveTick()
	{
		if (_state.IsAggro)
		{
			if (!_isChasingTarget)
			{
				BaseUnit? potentialTarget = FindValidTarget();
				if (potentialTarget != null)
				{
					_pivot = Pos;
					_target = potentialTarget;
					_isChasingTarget = true;
					_isDirectTarget = false;
				}
			}
			else if (_isChasingTarget && !_isDirectTarget)
			{
				if (_pivot.Distance(Pos) > ChaseDistance)
				{
					SetPathfinding(_pivot);
					_isGoingToPivot = true;
					_target = null;
					_isChasingTarget = false;
					_isDirectTarget = false;
				}
			}

			if (_isChasingTarget)
			{
				SetPathfinding(_target!.Pos);
			}
		}

		if (!_state.IsWalking) return;

		Vec2 target = CurrPath![CurrPathCheckpoint];

		if (target.Distance(Pos) <= MoveSpeed)
		{
			// Pos = target;
			CurrPathCheckpoint++;
			if (CurrPathCheckpoint >= CurrPath.Count)
			{
				if (_isGoingToPivot)
				{
					_isGoingToPivot = false;
					if (_walkGoal != null)
					{
						SetPathfinding(_walkGoal.Value);
					}
				}
				Halt();
			}
			return;
		}

		Vec2 direction = Pos.To(target).Unit;
		this.ApplyForce(direction * MoveSpeed);
	}

	public void Halt()
	{
		ClearVelocity();
		_state.IsWalking = false;
		CurrPath = null;
		_walkGoal = null;
	}

	public void SetAggro(bool aggro)
	{
		if (aggro == false)
		{
			if (_target != null)
			{
				_target._targetedByAmount--;
			}
			_target = null;
			_isDirectTarget = false;
			_isChasingTarget = false;
		}
		State.IsAggro = aggro;
	}

	public void Attack(IDestroyable target)
	{
		if (!(target is BaseUnit baseUnitTarget)) return;
		SetAggro(true);
		_target = baseUnitTarget;
		_isDirectTarget = true;
		_isChasingTarget = true;
	}

	public void AttackTick()
	{
		if (_cooldown > 0)
		{
			_cooldown--;
		}

		if (!State.IsAggro) return;
		if (_target == null) return;
		if (this.Pos.Distance(_target.Pos) - _target.Radius > AttackRange) return;

		_cooldown = AttackSpeed;
		((IDestroyable)_target).Damage(AttackDamage);

		if (_target.IsDestroyed)
		{
			SetAggro(false);
		}
	}

	private BaseUnit? FindValidTarget()
	{
		BaseUnit? target = null;
		float targetDistance = float.PositiveInfinity;
		int targetNumAttackers = int.MaxValue;

		foreach (BaseUnit unit in RtsEngine.Instance.State.Units)
		{
			if (_pivot.Distance(this.Pos) < ChaseDistance)
			{
				float distance = this.Pos.Distance(unit.Pos);

				if ((target == null) ||
					(unit._targetedByAmount < targetNumAttackers) ||
					(distance < targetDistance))
				{
					target = unit;
					targetDistance = distance;
					targetNumAttackers = unit._targetedByAmount;
				}
			}
		}

		return target;
	}
}
}
