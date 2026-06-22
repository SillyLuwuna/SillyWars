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
	public abstract float AggroRange { get; set; }

	private BaseUnit? _target;
	private bool _isDirectTarget;
	private int _cooldown;
	private Vec2? _pivot;
	private bool _isGoingToPivot;
	private Vec2? _walkGoal;
	private int _walkGoalCheckpoint;
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
		writer.Write(IsDestroyed);
		writer.Write(HitPoints);
		writer.Write(AttackDamage);
		writer.Write(AttackSpeed);
		writer.Write(AttackRange);
		writer.Write(ChaseDistance);
		writer.Write(AggroRange);
		writer.Write(MoveSpeed);
		writer.Write(State);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
		IsDestroyed = reader.Read<bool>();
		HitPoints = reader.Read<int>();
		AttackDamage = reader.Read<int>();
		AttackSpeed = reader.Read<int>();
		AttackRange = reader.Read<float>();
		ChaseDistance = reader.Read<float>();
		AggroRange = reader.Read<float>();
		MoveSpeed = reader.Read<float>();
		State = reader.Read<EntityState>();
	}

	public void SetGoal(Grid<Cell> map, Vec2 goal)
	{
		_walkGoal = goal;
		SetPathfinding(goal);
	}

	private bool HasWalkGoal
	{
		get => _walkGoal != null;
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
			UpdateMoveAggro();
		}

		if (!_state.IsWalking) return;

		Vec2 target = CurrPath![CurrPathCheckpoint];

		if (target.Distance(Pos) <= MoveSpeed)
		{
			Checkpoint();
			return;
		}

		Vec2 direction = Pos.To(target).Unit;
		this.ApplyForce(direction * MoveSpeed);
	}

	private void Checkpoint()
	{
		CurrPathCheckpoint++;

		if (HasTarget)
		{
			UpdateTargetPathfinding();
		}

		if (CurrPathCheckpoint >= CurrPath!.Count)
		{
			HandlePathArrival();
		}
	}

	private void UpdateTargetPathfinding()
	{
		SetPathfinding(_target!.Pos);
		if (CurrPath![1].Distance(Pos) <= MoveSpeed)
		{
			CurrPathCheckpoint++;
		}
	}

	private void HandlePathArrival()
	{
		if (_isGoingToPivot)
		{
			HandleArrivalAtPivot();
			return;
		}

		Halt();
	}

	private void HandleArrivalAtPivot()
	{
		_pivot = null;
		_isGoingToPivot = false;

		if (HasWalkGoal)
		{
			RestoreWalkGoal();
			return;
		}

		Halt();
	}

	private void ContinueWalkingToGoal()
	{
		_pivot = null;
		_isGoingToPivot = false;
		RestoreWalkGoal();
	}

	private void RestoreWalkGoal()
	{
		CurrPathCheckpoint = _walkGoalCheckpoint;
		SetPathfinding(_walkGoal!.Value);
	}

	private void ContinueTowardsCurrentGoal()
	{
		if (HasWalkGoal)
		{
			ContinueWalkingToGoal();
		}
		else
		{
			ReturnToPivot();
		}
	}

	private bool IsDirectTarget { get => _isDirectTarget; }
	private bool IsGoingToPivot { get => _isGoingToPivot; }
	private bool HasPivot { get => _pivot != null; }

	private void UpdateMoveAggro()
	{
		// if (HasWalkGoal) return; // to only aggro when arriving, still makes them go back and forth due to unit pushing

		if (!HasTarget)
		{
			BaseUnit? validTarget = FindValidTarget();
			if (validTarget != null)
			{
				UpdateIndirectTarget(validTarget);
			}
			else if (!IsGoingToPivot && HasPivot)
			{
				ContinueTowardsCurrentGoal();
				// ReturnToPivot();
			}
		}
		else if (!IsDirectTarget && !IsTargetInChaseDistance)
		{
			ContinueTowardsCurrentGoal();
			// ReturnToPivot();
		}

		if (HasTarget)
		{
			UpdateAttackMovement();
		}
	}

	private void UpdateAttackMovement()
	{
		if (IsInAttackRange)
		{
			PauseWalking();
			return;
		}

		ContinueWalking();

		if (!HasPath)
		{
			SetPathfinding(_target!.Pos);
		}
		else if (CurrPath!.Count == 2)
		{
			CurrPath[1] = _target!.Pos;
		}
	}

	private void UpdateIndirectTarget(BaseUnit target)
	{
		if (_pivot == null)
		{
			_walkGoalCheckpoint = CurrPathCheckpoint;
			_pivot = Pos;
		}
		_isGoingToPivot = false;
		_target = target;
		_isDirectTarget = false;
	}

	private void ReturnToPivot()
	{
		SetPathfinding(_pivot!.Value);
		_isGoingToPivot = true;
		_target = null;
		_isDirectTarget = false;
	}

	private bool IsTargetInChaseDistance
	{
		get => _pivot?.Distance(_target!.Pos) - AttackRange <= ChaseDistance;
	}

	private bool IsTargetInAggroRange
	{
		get => IsUnitInAggroRange(_target!);
	}

	private bool IsUnitInAggroRange(BaseUnit unit)
	{
		if (_pivot == null)
		{
			return this.Pos.Distance(unit.Pos) <= AggroRange;
		}

		return _pivot.Value.Distance(unit.Pos) <= AggroRange;
	}

	private bool IsInAttackRange
	{
		get => this.Pos.Distance(_target!.Pos) - _target.Radius <= AttackRange;
	}

	private bool HasTarget
	{
		get => _target != null;
	}

	private bool HasPath
	{
		get => !(CurrPath == null);
	}

	private void PauseWalking()
	{
		ClearVelocity();
		_state.IsWalking = false;
	}

	private void ContinueWalking()
	{
		_state.IsWalking = true;
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
			_pivot = null;
			_target = null;
			_isDirectTarget = false;
			_isGoingToPivot = false;
		}
		State.IsAggro = aggro;
	}

	public void Attack(IDestroyable target)
	{
		if (!(target is BaseUnit baseUnitTarget)) return;
		SetAggro(true);
		_target = baseUnitTarget;
		_isDirectTarget = true;
		SetPathfinding(_target.Pos);
	}

	public void AttackTick()
	{
		if (_cooldown > 0)
		{
			_cooldown--;
			return;
		}

		if (!State.IsAggro) return;
		if (!HasTarget) return;
		if (HandleEnemyDeath()) return;
		if (!IsInAttackRange) return;

		_cooldown = AttackSpeed;
		((IDestroyable)_target!).Damage(AttackDamage);

		HandleEnemyDeath();
	}

	private bool HandleEnemyDeath()
	{
		bool isTargetDestroyed = _target!.IsDestroyed;
		if (isTargetDestroyed)
		{
			if (_isDirectTarget)
			{
				SetAggro(false);
			}
			else
			{
				_target = null;
			}
		}
		return isTargetDestroyed;
	}

	private BaseUnit? FindValidTarget()
	{
		BaseUnit? target = null;
		float targetDistance = float.PositiveInfinity;
		int targetNumAttackers = int.MaxValue;

		foreach (BaseUnit unit in RtsEngine.Instance.State.Units)
		{
			if (unit.OwnerId == this.OwnerId) continue;
			if (unit.Id == this.Id) continue;
			if (!IsUnitInAggroRange(unit)) continue;
			if (unit._targetedByAmount > targetNumAttackers) continue;

			float distance = this.Pos.Distance(unit.Pos);

			if ((unit._targetedByAmount < targetNumAttackers) ||
				(unit._targetedByAmount == targetNumAttackers && distance < targetDistance))
			{
				target = unit;
				targetDistance = distance;
				targetNumAttackers = unit._targetedByAmount;
			}
		}

		return target;
	}
}
}
