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
	private int _attackCooldown;
	private Vec2? _pivot;
	private bool _isGoingToPivot;
	private Vec2? _walkGoal;
	private int _targetedByAmount;

	public abstract float MoveSpeed { get; set; }

	public Map.Path? CurrWalkPath { get; set; }
	public int CurrWalkPathCheckpoint { get; set; }

	private Units.State _state;
	public Units.State State { get => _state; set => _state = value; }

	protected event Action? WalkGoalReached;

	public BaseUnit(Vec2 pos, uint ownerId, float mass=1.0f, float radius=0.2f, float friction=1.0f) : base(pos, ownerId, mass, radius, friction)
	{
		IsDestroyed = false;
		_target = null;
		_attackCooldown = 0;
		_walkGoal = null;
		_isGoingToPivot = false;
		_targetedByAmount = 0;

		CurrWalkPath = null;
		_state = new State();
	}

	public override void Tick()
	{
		base.Tick();

		DecreaseCooldowns();

		if (_state.IsAggro)
		{
			UpdateMoveAggro();
		}

		if (HasTarget)
		{
			UpdateAttackMovement();
		}

		if (_state.IsWalking)
		{
			MoveTick();
		}

		if (State.IsAggro || State.Goal == Goal.Attack)
		{
			AttackTick();
		}
	}

	private void DecreaseCooldowns()
	{
		if (_attackCooldown > 0)
		{
			_attackCooldown--;
			return;
		}
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
		State = reader.Read<State>();
	}

	public virtual void SetGoal(Vec2 goal)
	{
		if (!SetWalkingGoal(goal)) return;
		State.Goal = Goal.Walk;
	}

	// to make the unit move towards long term goal
	protected bool SetWalkingGoal(Vec2 goal)
	{
		_walkGoal = goal;
		if (!SetPathfinding(goal))
		{
			State.Goal = Goal.None;
			_walkGoal = null;
			return false;
		}
		return true;
	}

	// to make the unit move towards an immediate goal
	private bool SetPathfinding(Vec2 goal)
	{
		CurrWalkPath = RtsEngine.Instance.State.PathFinder.GetPath(Pos, goal);
		if (CurrWalkPath.Count <= 1)
		{
			CurrWalkPath = null;
			_state.IsWalking = false;
			return false;
		}

		CurrWalkPathCheckpoint = 1;

		_state.IsWalking = true;
		return true;
	}

	protected void MoveTick()
	{
		// Console.WriteLine("============= MoveTick =============");
		// Console.WriteLine($"Id: {Id}");
		// Console.WriteLine($"Goal: {State.Goal}");
		// Console.WriteLine($"Aggro: {State.IsAggro}");
		// Console.WriteLine($"HasTarget: {HasTarget}");
		// Console.WriteLine($"IsWalking: {State.IsWalking}");
		// Console.WriteLine($"HasWalkGoal: {HasWalkGoal}");
		// Console.WriteLine($"IsGoingToPivot: {IsGoingToPivot}");
		// Console.WriteLine("============= MoveTick =============");
		Vec2 target = CurrWalkPath![CurrWalkPathCheckpoint];

		if (target.Distance(Pos) <= MoveSpeed)
		{
			// Console.WriteLine($"{Id}: Checkpoint ({CurrWalkPathCheckpoint})");
			Checkpoint();
			return;
		}

		Vec2 direction = Pos.To(target).Unit;
		this.ApplyForce(direction * MoveSpeed);
	}

	private void Checkpoint()
	{
		CurrWalkPathCheckpoint++;

		if (HasTarget)
		{
			UpdateTargetPathfinding();
		}

		if (CurrWalkPathCheckpoint >= CurrWalkPath!.Count)
		{
			HandlePathArrival();
		}
	}

	private void UpdateTargetPathfinding()
	{
		if (!SetPathfinding(_target!.Pos))
		{
			// Console.WriteLine($"{Id}: no path to target");
			if (State.Goal == Goal.Attack)
			{
				State.Goal = Goal.None;
			}
			_target = null;
			return;
		}

		if (CurrWalkPath![1].Distance(Pos) <= MoveSpeed)
		{
			CurrWalkPathCheckpoint++;
			// Console.WriteLine($"{Id}: (extra) Checkpoint ({CurrWalkPathCheckpoint})");
		}
	}

	private bool HasWalkGoal { get => _walkGoal != null; }

	private void HandlePathArrival()
	{
		if (_isGoingToPivot)
		{
			// Console.WriteLine($"{Id}: Arrived at pivot");
			HandleArrivalAtPivot();
			return;
		}

		if (HasTarget)
		{
			// Console.WriteLine($"{Id}: Arrived at target");
			PauseWalking();
			return;
		}

		if (HasWalkGoal)
		{
			// Console.WriteLine($"{Id}: Arrived at walk goal");
			Halt();
			OnWalkGoalReached();
			return;
		}
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

		PauseWalking();
	}

	private void ContinueWalkingToGoal()
	{
		// Console.WriteLine($"{Id}: Continuing walking to goal");
		_pivot = null;
		_isGoingToPivot = false;
		RestoreWalkGoal();
	}

	private void RestoreWalkGoal()
	{
		SetPathfinding(_walkGoal!.Value);
	}

	private void ContinueTowardsCurrentGoal()
	{
		if (State.Goal == Goal.None)
		{
			ReturnToPivot();
		}
		else
		{
			ContinueWalkingToGoal();
		}
	}

	private bool IsGoingToPivot { get => _isGoingToPivot; }
	private bool HasPivot { get => _pivot != null; }

	private void UpdateMoveAggro()
	{
		if (!HasTarget)
		{
			TryFindTarget();
		}
		else if (!IsTargetInChaseDistance)
		{
			ContinueTowardsCurrentGoal();
			// ReturnToPivot();
		}
	}

	private void TryFindTarget()
	{
		BaseUnit? validTarget = FindValidTarget();
		if (validTarget != null)
		{
			// Console.WriteLine($"{Id}: Found target");
			UpdateIndirectTarget(validTarget);
		}
		else if (!IsGoingToPivot && HasPivot)
		{
			// Console.WriteLine($"{Id}: No targets");
			ContinueTowardsCurrentGoal();
			// ReturnToPivot();
		}
	}

	private void UpdateAttackMovement()
	{
		if (IsInAttackRange)
		{
			// Console.WriteLine($"{Id}: In attack range");
			PauseWalking();
			return;
		}

		ContinueWalking();
	}

	private void UpdateIndirectTarget(BaseUnit target)
	{
		if (_pivot == null)
		{
			_pivot = Pos;
		}
		_isGoingToPivot = false;
		_target = target;
		SetPathfinding(_target.Pos);
	}

	private void ReturnToPivot()
	{
		// Console.WriteLine($"{Id}: returning to pivot");
		SetPathfinding(_pivot!.Value);
		_isGoingToPivot = true;
		_target = null;
	}

	private bool IsTargetInChaseDistance
	{
		// get => _pivot?.Distance(_target!.Pos) - AttackRange <= ChaseDistance;
		get => Pos.Distance(_target!.Pos) - AttackRange <= ChaseDistance;
	}

	private bool IsTargetInAggroRange
	{
		get => IsUnitInAggroRange(_target!);
	}

	private bool IsUnitInAggroRange(BaseUnit unit)
	{
		// if (_pivot == null)
		// {
		return this.Pos.Distance(unit.Pos) <= AggroRange;
		// }
		//
		// return _pivot.Value.Distance(unit.Pos) <= AggroRange;
	}

	private bool IsInAttackRange
	{
		get => this.Pos.Distance(_target!.Pos) - _target.Radius <= AttackRange;
	}

	private bool HasTarget { get => _target != null; }

	private bool HasPath { get => !(CurrWalkPath == null); }

	private void PauseWalking()
	{
		ClearVelocity();
		_state.IsWalking = false;
	}

	private void ContinueWalking()
	{
		_state.IsWalking = true;
	}

	public virtual void Halt()
	{
		ClearVelocity();
		_state.IsWalking = false;
		CurrWalkPath = null;
		_walkGoal = null;

		if (State.Goal == Goal.Walk)
		{
			_state.Goal = Goal.None;
		}
	}

	public void SetAggro(bool aggro)
	{
		if (aggro == false)
		{
			if (_target != null)
			{
				_target._targetedByAmount--;
			}
			// Console.WriteLine($"{Id}: Dropping aggro");
			_pivot = null;
			_target = null;
			_isGoingToPivot = false;
		}
		State.IsAggro = aggro;
	}

	public virtual void Attack(IDestroyable target)
	{
		if (!(target is BaseUnit baseUnitTarget)) return;
		_target = baseUnitTarget;
		if (!SetPathfinding(_target.Pos))
		{
			_target = null;
			State.Goal = Goal.None;
			return;
		}
		State.Goal = Goal.Attack;
	}

	protected void AttackTick()
	{
		if (!HasTarget) return;
		if (_target!.IsDestroyed)
		{
			HandleEnemyDeath();
			return;
		}
		if (!IsInAttackRange) return;

		_attackCooldown = AttackSpeed;
		((IDestroyable)_target!).Damage(AttackDamage);

		if (_target!.IsDestroyed)
		{
			HandleEnemyDeath();
			return;
		}
	}

	private void HandleEnemyDeath()
	{
		if (State.Goal == Goal.Attack)
		{
			State.Goal = Goal.None;
		}

		// Console.WriteLine($"{Id}: Target died");
		_target = null;
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

	public static BaseUnit FromType(Type type, uint ownerId, Vec2 pos)
	{
		switch (type)
		{
			case (Type.Worker):
				return new Worker(pos, ownerId);
			case (Type.Knight):
				return new Knight(pos, ownerId);
			default:
				throw new ArgumentException($"Unknown unit type {type}");
		}
	}

	private void OnWalkGoalReached()
	{
		WalkGoalReached?.Invoke();
	}
}
}
