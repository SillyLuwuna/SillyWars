using System;
using System.Collections.Generic;
using System.Linq;
using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using RtsEngine.Math;
using RtsEngine.Structures;

namespace RtsEngine.Units
{

public class Worker : BaseUnit, IBuilder
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
	public const float BaseMoveSpeed = 0.20f;

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

	public int BuildSpeed { get; set; }

	private BaseStructure? _structure;
	Vec2Int? _closestReachableTile;
	private bool _goingTowardsStructure;

	private int _buildCooldown;

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
		
		State.Changed += OnStateChange;
		WalkGoalReached += OnWalkGoalReached;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);

		writer.Write(BuildSpeed);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);

		BuildSpeed = reader.Read<int>();

		Init();
	}

	public override UnitType UnitType { get => UnitType.Worker; }

	private void OnStateChange(object? sender, StateEventArgs args)
	{
		// if (args.OldState.Goal != args.NewState.Goal)
		// {
		// 	Console.WriteLine($"goal: {args.OldState.Goal} -> {args.NewState.Goal}");
		// }
		// if (args.OldState.IsWalking != args.NewState.IsWalking)
		// {
		// 	Console.WriteLine($"walking: {args.OldState.IsWalking} -> {args.NewState.IsWalking}");
		// }
		// if (args.OldState.IsAggro != args.NewState.IsAggro)
		// {
		// 	Console.WriteLine($"aggro: {args.OldState.IsAggro} -> {args.NewState.IsAggro}");
		// }

		if (args.OldState.Goal == Goal.Build && args.NewState.Goal != Goal.Build)
		{
			StopBuilding();
		}
	}

	public void Build(BaseStructure structure)
	{
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
			return;
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
	}
}
}
