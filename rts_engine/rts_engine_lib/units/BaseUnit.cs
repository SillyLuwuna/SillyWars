using System.IO;
using RtsEngine.Data;
using RtsEngine.Math;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using System;
using RtsEngine.Physics;
using System.Collections.Generic;
using RtsEngine.Structures;
using RtsEngine.Resources;

namespace RtsEngine.Units
{

public abstract class BaseUnit : PhysicsObject, ISerializable, IMovable, IAttacker, IDestroyable, IValuable
{
	private const float StructurePriority = 1.0f;
	private const float UnitPriority = 10.0f;
	private const float DistancePriorityWeight = 1.0f;
	private const float NumAttackersPriorityWeight = 2.0f;

	public bool IsDestroyed { get; set; }

	public abstract int HitPoints { get; set; }

	public abstract int AttackDamage { get; set; }
	public abstract int AttackSpeed { get; set; }
	public abstract float AttackRange { get; set; }
	public abstract float ChaseDistance { get; set; }
	public abstract float AggroRange { get; set; }

	public abstract float MoveSpeed { get; set; }

	public abstract int ProductionTime { get; set; }

	public abstract ResourceStack Cost { get; }
	public bool IsPaid { get; set; }

	public int TargetedByNum { get; set; }

	public Map.Path? CurrWalkPath { get; set; }
	public int CurrWalkPathCheckpoint { get; set; }

	private Units.State _state;
	public Units.State State { get => _state; set => _state = value; }

	private Vec2? _walkGoal;
	protected event Action? WalkGoalReached;

	private IDestroyable? _targetGoal;

	private int _attackCooldown;
	private IDestroyable? _target;
	private BaseUnit? _targetUnit;
	private BaseStructure? _targetStructure;
	private Vec2Int? _targetStructureAttackTile;
	private Vec2? _targetStructureAttackPos;
	private Vec2? _pivot;
	private bool _isGoingToPivot;

	// for client
	private bool _attacked;
	public Vec2? NextWaypoint;
	// for client

	public BaseUnit(Vec2 pos, uint ownerId, float mass=1.0f, float radius=0.2f, float friction=1.0f) : base(pos, ownerId, mass, radius, friction)
	{
		IsDestroyed = false;
		TargetedByNum = 0;

		IsPaid = false;

		_target = null;
		_targetUnit = null;
		_targetStructure = null;
		_attackCooldown = 0;
		_targetGoal = null;
		_walkGoal = null;
		_isGoingToPivot = false;

		CurrWalkPath = null;
		_state = new State();

		_attacked = false;
	}

	public override void Tick()
	{
		base.Tick();

		_attacked = false;

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

	public bool Attacked { get => _attacked; }

	protected virtual void DecreaseCooldowns()
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
		writer.Write(ProductionTime);
		writer.Write(State);
		writer.Write(_attacked);

		NextWaypoint = null;
		if (!(CurrWalkPath == null) && CurrWalkPathCheckpoint < CurrWalkPath.Count )
		{
			NextWaypoint = CurrWalkPath[CurrWalkPathCheckpoint];
		}
		writer.Write(NextWaypoint);
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
		ProductionTime = reader.Read<int>();
		State = reader.Read<State>();

		_attacked = reader.Read<bool>();
		NextWaypoint = reader.Read<Vec2?>();
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

	private bool TargetIsUnit { get => _targetUnit != null; }
	private bool TargetIsStructure { get => _targetStructure != null; }

	private void Checkpoint()
	{
		CurrWalkPathCheckpoint++;

		if (HasTarget && TargetIsUnit)
		{
			UpdateTargetUnitPathfinding();
		}

		if (CurrWalkPathCheckpoint >= CurrWalkPath!.Count)
		{
			HandlePathArrival();
		}
	}

	private void ClearTarget()
	{
		if (_target != null)
		{
			_target!.TargetedByNum--;
		}

		_target = null;
		_targetUnit = null;
		_targetStructure = null;
		_targetStructureAttackTile = null;
		_targetStructureAttackPos = null;
	}

	private void RestoreAttackGoal()
	{
		SetTarget(_targetGoal!);

		if (!HasTarget)
		{
			StopAttackGoal();
		}
	}

	private void UpdateTargetUnitPathfinding()
	{
		if (!SetPathfinding(_targetUnit!.Pos))
		{
			// Console.WriteLine($"{Id}: no path to target");
			if (State.Goal == Goal.Attack && IsTargettingGoalTarget)
			{
				StopAttackGoal();
			}
			ClearTarget();
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
		}
	}

	private void TryFindTarget()
	{
		IDestroyable? targetFound = FindTarget();
		if (targetFound != null)
		{
			// Console.WriteLine($"{Id}: Found target");
			SetTarget(targetFound);
		}
		else if (!IsGoingToPivot && HasPivot)
		{
			// Console.WriteLine($"{Id}: No targets");
			ContinueTowardsCurrentGoal();
		}
	}

	private void UpdateAttackMovement()
	{
		if (IsTargetInAttackRange)
		{
			// Console.WriteLine($"{Id}: In attack range");
			PauseWalking();
			return;
		}

		ContinueWalking();
	}

	private void ReturnToPivot()
	{
		// Console.WriteLine($"{Id}: returning to pivot");
		SetPathfinding(_pivot!.Value);
		_isGoingToPivot = true;
		ClearTarget();
	}

	private bool IsTargetInChaseDistance
	{
		get
		{
			if (TargetIsUnit) return Pos.Distance(_targetUnit!.Pos) - AttackRange <= ChaseDistance;
			if (TargetIsStructure) return true;

			return false;
		}
	}

	private bool IsUnitInAggroRange(BaseUnit unit)
	{
		return IsInAggroRange(unit.Pos);
	}

	private bool IsStructureInAggroRange(BaseStructure structure)
	{
		// could be cached if done smart
		Vec2Int? structureTile = GetClosestReachableTileToStructure(structure);
		if (structureTile == null) return false;

		Vec2 structurePos = RtsEngine.Instance.State.Map.WorldSpaceFromCellPos(structureTile.Value);

		return IsInAggroRange(structurePos);
	}

	private bool IsInAggroRange(Vec2 targetPos)
	{
		return this.Pos.Distance(targetPos) <= AggroRange;
	}

	private bool IsTargetInAttackRange
	{
		get
		{
			if (TargetIsUnit)
			{
				return this.Pos.Distance(_targetUnit!.Pos) - _targetUnit.Radius <= AttackRange;
			}
			if (TargetIsStructure)
			{
				return RtsEngine.Instance.State.Map.CellPosFromWorldSpace(this.Pos) == _targetStructureAttackTile;
			}

			return false;
		}
	}

	// private bool HasTarget { get => _target != null; }
	protected bool HasTarget { get => _target != null; }

	protected bool HasPath { get => !(CurrWalkPath == null); }

	protected void PauseWalking()
	{
		ClearVelocity();
		_state.IsWalking = false;
	}

	protected void ContinueWalking()
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
			ClearTarget();
			_pivot = null;
			_isGoingToPivot = false;
			if (HasWalkGoal)
			{
				RestoreWalkGoal();
			}
		}
		else
		{
			if (State.Goal == Goal.Gather)
			{
				State.Goal = Goal.None;
			}
		}
		State.IsAggro = aggro;
	}

	public virtual void Attack(IDestroyable target)
	{
		_targetGoal = target;
		SetTarget(target);
		State.Goal = Goal.Attack;

		if (!HasTarget)
		{
			StopAttackGoal();
		}
	}

	private void SetTarget(IDestroyable target)
	{
		ClearTarget();

		_target = target;
		_target.TargetedByNum++;

		if (_pivot == null)
		{
			_pivot = Pos;
		}
		_isGoingToPivot = false;

		if (target is BaseUnit unitTarget)
		{
			SetUnitTarget(unitTarget);
		}
		else if (target is BaseStructure structureTarget)
		{
			SetStructureTarget(structureTarget);
		}
		else
		{
			ClearTarget();
		}
	}

	private void SetUnitTarget(BaseUnit target)
	{
		_targetUnit = target;
		_targetStructure = null;

		if (!SetPathfinding(target.Pos))
		{
			ClearTarget();
		}
	}

	private void SetStructureTarget(BaseStructure structure)
	{
		_targetUnit = null;
		_targetStructure = structure;

		_targetStructureAttackTile = GetClosestReachableTileToStructure(structure);
		if (_targetStructureAttackTile == null)
		{
			ClearTarget();
			return;
		}

		_targetStructureAttackPos = RtsEngine.Instance.State.Map.WorldSpaceFromCellPos(_targetStructureAttackTile.Value);
		SetPathfinding(_targetStructureAttackPos.Value);
	}

	private void StopAttackGoal()
	{
		State.Goal = Goal.None;
		_targetGoal = null;
	}

	protected void AttackTick()
	{
		if (_attackCooldown > 0) return;
		if (!HasTarget) return;
		if (_target!.IsDestroyed)
		{
			HandleTargetDestruction();
			return;
		}
		if (!IsTargetInAttackRange) return;

		_attackCooldown = AttackSpeed;
		_target.Damage(AttackDamage);
		_attacked = true;

		if (_target.IsDestroyed)
		{
			HandleTargetDestruction();
			return;
		}
	}

	private void HandleTargetDestruction()
	{
		// Console.WriteLine($"{Id}: Target died");
		if (State.Goal == Goal.Attack)
		{
			if (IsTargettingGoalTarget)
			{
				Console.WriteLine("Stopping attack goal");
				// Halt();
				StopAttackGoal();
				ClearTarget();
			}
			else
			{
				Console.WriteLine("Restoring attack goal");
				RestoreAttackGoal();
			}
		}
		else
		{
			ClearTarget();
		}
	}

	private bool IsTargettingGoalTarget { get => _targetGoal!.Equals(_target); }

	protected IDestroyable? FindTarget()
	{
		IDestroyable? target = null;
		float targetPriority = -1;

		foreach (IDestroyable entity in RtsEngine.Instance.State.Destroyables)
		{
			float priority = TargetPriority(entity);
			if (priority < 0) continue;

			if (priority > targetPriority)
			{
				target = entity;
				targetPriority = priority;
			}
		}

		return target;
	}

	protected float TargetPriority(IDestroyable target)
	{
		if (target.OwnerId == this.OwnerId) return -1;
		if (target.Id == this.Id) return -1;

		Vec2 targetPos;
		float typePriority; // could be refined further into more specific types priority

		if (target is BaseStructure structure)
		{
			Vec2? structurePos = GetClosestReachablePointToStructure(structure);
			if (structurePos == null) return -1;

			targetPos = structurePos.Value;
			typePriority = StructurePriority;
		}
		else if (target is BaseUnit unit)
		{
			targetPos = unit.Pos;
			typePriority = UnitPriority;
		}
		else
		{
			return -1;
		}

		if (!IsInAggroRange(targetPos)) return -1;

		float distance = this.Pos.Distance(targetPos);
		int numAttackers = target.TargetedByNum;

		float distancePriority = (1.0f / (1.0f + distance)) * DistancePriorityWeight;
		float attackersPriority = (1.0f / (1.0f + (float)numAttackers)) * NumAttackersPriorityWeight;

		return (distancePriority + attackersPriority + typePriority);
	}

	public static BaseUnit FromUnitType(UnitType type, uint ownerId, Vec2 pos)
	{
		return type switch
		{
			UnitType.Worker => new Worker(pos, ownerId),
			UnitType.Knight => new Knight(pos, ownerId),
			_ => throw new ArgumentException($"Unknown unit type {type}")
		};
	}

	public static BaseUnit Dummy(UnitType type)
	{
		return FromUnitType(type, ~0u, Vec2.Zero);
	}

	public abstract UnitType UnitType { get; }

	private void OnWalkGoalReached()
	{
		Halt();
		WalkGoalReached?.Invoke();
	}

	protected Vec2? GetClosestReachablePointToStructure(BaseStructure structure)
	{
		return GetClosestReachableTile(structure.SurroundingTiles)?.Last;
	}

	protected Vec2Int? GetClosestReachableTileToStructure(BaseStructure structure)
	{
		Map.Path? path = GetClosestReachableTile(structure.SurroundingTiles);
		if (path == null) return null;
		return RtsEngine.Instance.State.Map.CellPosFromWorldSpace(path.Last);
	}

	protected Map.Path? GetShortestReachablePathToStructure(BaseStructure structure)
	{
		return GetClosestReachableTile(structure.SurroundingTiles);
	}

	protected Map.Path? GetClosestReachableTile(List<Vec2Int> tiles)
	{
		Grid<Cell> map = RtsEngine.Instance.State.Map;
		PathFinder pathFinder = RtsEngine.Instance.State.PathFinder;

		// Vec2Int? bestTile = null;
		Map.Path? bestPath = null;
		float bestDistance = float.PositiveInfinity;

		foreach (Vec2Int tile in tiles)
		{
			if (!map.ContainsPos(tile)) continue;
			if (!map[tile].IsWalkable) continue;

			Vec2 tilePos = map.WorldSpaceFromCellPos(tile);
			Map.Path path = pathFinder.GetPath(this.Pos, tilePos);

			if (path.Count == 0) continue;

			float pathLength = path.Length;

			if (pathLength < bestDistance)
			{
				bestDistance = pathLength;
				bestPath = path;
				// bestTile = tile;
			}
		}

		return bestPath;
		// return bestTile;
	}
}
}
