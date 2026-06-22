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
	public override int HitPoints { get; set; }
	public override int AttackDamage { get; set; }
	public override int AttackSpeed { get; set; }
	public override float AttackRange { get; set; }
	public override float ChaseDistance { get; set; }
	public override float AggroRange { get; set; }
	public override float MoveSpeed { get; set; }

	public int BuildSpeed { get; set; }

	private BaseStructure? _structure;
	Vec2Int? _closestReachableTile;
	private bool _goingTowardsStructure;

	private int _buildCooldown;

	public Worker(Vec2 pos, uint ownerId) : base(pos, ownerId)
	{
		HitPoints = 1;
		AttackDamage = 1;
		AttackSpeed = 15;
		AttackRange = 0.25f;
		ChaseDistance = 2.0f;
		AggroRange = 2.0f;
		MoveSpeed = 0.18f;

		Radius = 0.2f;
		Mass = 1.0f;
		Friction = 1.0f;

		BuildSpeed = 20;
		_buildCooldown = 0;
		_structure = null;
		_closestReachableTile = null;
		_goingTowardsStructure = false;
		
		State.Changed += OnStateChange;
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
	}

	private void OnStateChange(object? sender, StateEventArgs args)
	{
		if (args.OldState.Goal == Goal.Build && args.NewState.Goal != Goal.Build)
		{
			Console.WriteLine("Building interrupted");
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

	private void DecreaseCooldowns()
	{
		if (_buildCooldown > 0)
		{
			_buildCooldown--;
		}
	}

	private void TickBuild()
	{
		if (_structure!.IsDestroyed)
		{
			Console.WriteLine("Can't build: structure was destroyed");
			StopBuilding();
			return;
		}

		if (_goingTowardsStructure) return;
		if (_buildCooldown > 0) return;

		if (!IsInRangeToBuild)
		{
			if (!GoToStructureRange())
			{
				Console.WriteLine("Can't build: structure unreachable");
				StopBuilding();
			}

			return;
		}

		if (IsBuildingNewStructure)
		{
			if (_structure.IsStructureAreaObstructed)
			{
				Console.WriteLine("Can't build: structure area obstructed");
				StopBuilding();
			}
			else
			{
				Console.WriteLine("Starting build");
				_structure.StartBuilding();
				_buildCooldown = BuildSpeed;
			}

			return;
		}

		if (_structure.IsFullyBuilt)
		{
			Console.WriteLine("Structure fully built!");
			StopBuilding();
			return;
		}

		Console.WriteLine("Starting build");
		_structure.DoBuildWork();
		_buildCooldown = BuildSpeed;
	}
	
	private bool IsBuildingNewStructure { get => _structure!.HasBuildingStarted; }

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
		List<Vec2Int> surroundingTiles = _structure!.GetSurroundingTiles();
		_closestReachableTile = GetClosestReachableTile(surroundingTiles);

		bool hasTile = _closestReachableTile != null;

		_goingTowardsStructure = hasTile;
		return hasTile;
	}

	private Vec2Int? GetClosestReachableTile(List<Vec2Int> tiles)
	{
		Grid<Cell> map = RtsEngine.Instance.State.Map;
		PathFinder pathFinder = RtsEngine.Instance.State.PathFinder;

		List<Vec2Int> candidateTiles = tiles
			.Where(tile => map.ContainsPos(tile))
			.Where(tile => map[tile].IsWalkable)
			.OrderBy(tile => map.WorldSpaceFromCellPos(tile).Distance(this.Pos))
			.ToList();

		foreach (Vec2Int tile in candidateTiles)
		{
			if (pathFinder.HasPath(this.Pos, map.WorldSpaceFromCellPos(tile))) return tile;
		}

		return null;
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
}
}
