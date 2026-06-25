using System;
using System.Collections.Generic;
using System.Linq;
using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using RtsEngine.Math;
using RtsEngine.Resources;
using RtsEngine.Structures;

namespace RtsEngine.Units
{

public class Worker : BaseUnit, IBuilder, IGatherer
{
	public const float BaseRadius = 0.2f;
	public const float BaseMass = 1.0f;
	public const float BaseFriction = 1.0f;

	public const int BaseHitPoints = 5;
	public const int BaseAttackDamage = 1;
	public const int BaseAttackSpeed = 20;
	public const float BaseAttackRange = BaseRadius + 0.1f;
	public const float BaseChaseDistance = 3.0f;
	public const float BaseAggroRange = 3.0f;
	public const float BaseMoveSpeed = 0.15f;

	public const int BaseProductionTime = 20 * 10;

	public const int BaseBuildSpeed = 20;

	public override int HitPoints { get; set; }
	public override int AttackDamage { get; set; }
	public override int AttackSpeed { get; set; }
	public override float AttackRange { get; set; }
	public override float ChaseDistance { get; set; }
	public override float AggroRange { get; set; }
	public override float MoveSpeed { get; set; }
	public override int ProductionTime { get; set; }

	public override ResourceStack Cost { get => new ResourceStack(Resource.Gold, 20); }


	public int WorkPerGather { get => 1; }
	public float GatherRange { get => BaseRadius + 0.2f; }

	private IGatherable? _gatherableGoal;
	private ResourceStack _resourceGathered;
	private Castle? _nearestCastle;


	public int BuildSpeed { get; set; }

	private BaseStructure? _structure;
	Vec2Int? _closestReachableTile;
	private bool _goingTowardsStructure;
	private int _buildCooldown;

	private bool _isGathering;
	private bool _isRetrieving;

	public Worker(Vec2 pos, uint ownerId) : base(pos, ownerId, BaseMass, BaseRadius, BaseFriction)
	{
		HitPoints = BaseHitPoints;
		AttackDamage = BaseAttackDamage;
		AttackSpeed = BaseAttackSpeed;
		AttackRange = BaseAttackRange;
		ChaseDistance = BaseChaseDistance;
		AggroRange = BaseAggroRange;
		MoveSpeed = BaseMoveSpeed;
		ProductionTime = BaseProductionTime;

		BuildSpeed = BaseBuildSpeed;

		Init();
	}

	private void Init()
	{
		_buildCooldown = 0;
		_structure = null;
		_closestReachableTile = null;
		_goingTowardsStructure = false;

		_gatherableGoal = null;
		_resourceGathered = new ResourceStack(Resource.None, 0);

		_isGathering = false;
		_isRetrieving = false;
		
		State.Changed += OnStateChange;
		WalkGoalReached += OnWalkGoalReached;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);

		writer.Write(BuildSpeed);
		writer.Write(IsGathering);
		writer.Write(IsRetrieving);
		writer.Write(_resourceGathered);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);

		Init();

		BuildSpeed = reader.Read<int>();
		_isGathering = reader.Read<bool>();
		_isRetrieving = reader.Read<bool>();
		_resourceGathered = reader.Read<ResourceStack>();
	}

	public override UnitType UnitType { get => UnitType.Worker; }

	public bool IsGathering { get => (State.Goal == Goal.Gather) && _isGathering; }
	public bool IsRetrieving { get => (State.Goal == Goal.Gather) && _isRetrieving; }
	public ResourceStack Holding { get => _resourceGathered; }

	private void OnStateChange(object? sender, StateEventArgs args)
	{
		if (args.OldState.Goal == Goal.Build && args.NewState.Goal != Goal.Build)
		{
			StopBuilding();
		}
	}

	public void Build(BaseStructure structure)
	{
		// if (!((IValuable)structure).TryPay()) return;
		// if (!structure.IsPaid && !structure.HasBuildingStarted &&
		// 	!RtsEngine.Instance.State.TryTakeResource(structure.Cost, this.OwnerId))
		// 	return;

		_structure = structure;
		State.Goal = Goal.Build;
		_closestReachableTile = null;
		_goingTowardsStructure = false;
	}

	public override void Tick()
	{
		base.Tick();

		DecreaseCooldowns();

		if (State.Goal == Goal.None) return;

		if (State.Goal == Goal.Build)
		{
			TickBuild();
		}
		else if (State.Goal == Goal.Gather)
		{
			if (_isRetrieving)
			{
				TickRetrieve();
			}
			else
			{
				TickGather();
			}
		}
	}

	protected override void DecreaseCooldowns()
	{
		base.DecreaseCooldowns();

		if (_buildCooldown > 0)
		{
			_buildCooldown--;
		}
	}

	private void TickBuild()
	{
		if (_structure!.IsDestroyed)
		{
			StopBuilding();
			return;
		}

		if (HasTarget) return;
		if (_goingTowardsStructure) return;
		if (_buildCooldown > 0) return;

		if (!IsInRangeToBuild)
		{
			if (!GoToStructureRange())
			{
				StopBuilding();
			}

			return;
		}

		if (IsBuildingNewStructure)
		{
			if (_structure.IsAreaObstructed)
			{
				StopBuilding();
			}
			else
			{
				if (!((IValuable)_structure).TryPay()) return;
				_structure.StartBuilding();
				_buildCooldown = BuildSpeed;
			}

			return;
		}

		if (_structure.IsFullyBuilt)
		{
			StopBuilding();
			return;
		}

		_structure.DoBuildWork();
		_buildCooldown = BuildSpeed;
	}
	
	private bool IsBuildingNewStructure { get => !_structure!.HasBuildingStarted; }

	private bool HasClosestReachableTile { get => _closestReachableTile != null; }

	private bool IsInRangeToBuild
	{
		get
		{
			if (!HasClosestReachableTile) return false;

			return RtsEngine.Instance.State.Map.CellPosFromWorldSpace(this.Pos) == _closestReachableTile;
		}
	}

	private bool GoToStructureRange()
	{
		_closestReachableTile = GetClosestReachableTileToStructure(_structure!);

		bool hasTile = _closestReachableTile != null;

		if (hasTile)
		{
			SetWalkingGoal(RtsEngine.Instance.State.Map.WorldSpaceFromCellPos(_closestReachableTile!.Value));
		}

		_goingTowardsStructure = hasTile;
		return hasTile;
	}

	private void StopBuilding()
	{
		if (State.Goal == Goal.Build)
		{
			State.Goal = Goal.None;
			return;
		}

		_structure = null;
		_closestReachableTile = null;
		_goingTowardsStructure = false;
	}

	private void OnWalkGoalReached()
	{
		if (State.Goal == Goal.Build)
		{
			_goingTowardsStructure = false;
		}
		else if (State.Goal == Goal.Gather)
		{
			if (_isRetrieving)
			{
				ReachedRetrievalDestination();
			}
		}
	}

	private void ReachedRetrievalDestination()
	{
		// give player the resources :3
		_nearestCastle!.DeliverResource(_resourceGathered);

		_resourceGathered = new ResourceStack(Resource.None, 0);
		_isRetrieving = false;
	}

	public void Gather(IGatherable gatherable)
	{
		Halt();
		SetAggro(false);
		State.Goal = Goal.Gather;
		_gatherableGoal = gatherable;
		_isGathering = false;
		_isRetrieving = false;
	}

	private void TickGather()
	{
		if (!State.IsWalking && !_gatherableGoal!.IsInGatheringRange(this))
		{
			SetWalkingGoal(_gatherableGoal.Pos);
			return;
		}

		if (!_gatherableGoal!.IsInGatheringRange(this)) return;

		Halt();

		_resourceGathered = _gatherableGoal!.TryGather(this);
		_isGathering = false;

		if (_resourceGathered.Resource == Resource.None)
		{
			return;
		}

		if (_resourceGathered.Amount <= 0)
		{
			_isGathering = true;
			return;
		}

		_isRetrieving = true;
	}

	private void TickRetrieve()
	{
		if (State.IsWalking) return;

		Vec2? castlePos = GetClosestCastle();
		if (castlePos == null)
		{
			State.Goal = Goal.None;
			return;
		}

		SetWalkingGoal(castlePos.Value);

		// on arrival:
	}

	private Vec2? GetClosestCastle()
	{
		List<Castle> castles = RtsEngine.Instance.State.Structures.OfType<Castle>().ToList();

		Vec2? shortest = null;
		float bestDistance = float.PositiveInfinity;

		foreach (Castle castle in castles)
		{
			if (!castle.IsBuilt) continue;
			if (castle.OwnerId != this.OwnerId) continue;

			Path? path = GetShortestReachablePathToStructure(castle);
			if (path == null) continue;

			float currDistance = path.Length;
			
			if (currDistance < bestDistance)
			{
				_nearestCastle = castle;
				bestDistance = currDistance;
				shortest = path.Last;
			}
		}

		return shortest;
	}
}
}
