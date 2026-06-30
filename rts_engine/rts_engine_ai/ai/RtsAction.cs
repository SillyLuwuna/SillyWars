using System.Linq;
using RtsEngine.Commands;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using RtsEngine.Math;
using RtsEngine.Resources;
using RtsEngine.Structures;
using RtsEngine.Units;

namespace RtsEngine.AI
{

public enum RtsAction
{
	TrainWorker,
	TrainKnight,

	BuildBarracks,
	BuildCastle,

	Attack,
	Defend,

	MineGold,
	BuildUnfinishedStructures,

	Wait,
}

public class RtsActionUtils
{
	// private enum Direction
	// {
	// 	Left,
	// 	Right,
	// 	Up,
	// 	Down
	// }
	
	// private Queue<Tuple<BaseStructure, Direction>> _buildLocations;

	private uint _playerId;
	private bool _start;
	private BaseStructure _baseStart = null!;

	public RtsActionUtils(uint playerId)
	{
		_playerId = playerId;
		_start = true;

		// _buildLocations = new Queue<Tuple<BaseStructure, Direction>>();
	}

	public void Update(WorldState state)
	{
		if (_start)
		{
			_start = false;
			foreach (BaseStructure structure in state.Structures)
			{
				if (structure.OwnerId != _playerId) continue;
				if (structure is Castle)
				{
					_baseStart = structure;
				}
			}
		}
	}

	// private void GenerateChildren(BaseStructure structure)
	// {
		// _buildLocations.Enqueue(new Tuple<BaseStructure, Direction>(structure, Direction.Left));
		// _buildLocations.Enqueue(new Tuple<BaseStructure, Direction>(structure, Direction.Right));
		// _buildLocations.Enqueue(new Tuple<BaseStructure, Direction>(structure, Direction.Up));
		// _buildLocations.Enqueue(new Tuple<BaseStructure, Direction>(structure, Direction.Down));
	// }

	public ICommand? ActionToCommand(RtsState currState, WorldState currWorld, RtsAction action)
	{
		switch (action)
		{
			case RtsAction.TrainWorker:
				return TrainWorker(currWorld);
			case RtsAction.TrainKnight:
				return TrainKnight(currWorld);
			case RtsAction.BuildBarracks:
				return BuildBarracks(currWorld);
			case RtsAction.BuildCastle:
				return BuildCastle(currWorld);
			case RtsAction.Attack:
				return Attack(currState, currWorld);
			case RtsAction.Defend:
				return Defend(currState, currWorld);
			case RtsAction.MineGold:
				return MineGold(currWorld);
			case RtsAction.BuildUnfinishedStructures:
				return BuildUnfinishedStructure(currWorld);
			case RtsAction.Wait:
				return Wait(currWorld);
			default:
				return null;
		}
	}

	private ICommand? TrainWorker(WorldState state)
	{
		if (state.GetResource(_playerId, Resource.Gold) < BaseUnit.Dummy(UnitType.Worker).Cost.Amount) return null;

		IEnumerable<Castle> playerCastles = state.Structures.OfType<Castle>().Where(c => c.OwnerId != _playerId);

		Castle? minProductionCastle = null;
		int minProduction = int.MaxValue;

		foreach (Castle castle in playerCastles)
		{
			if (castle.OwnerId != _playerId) continue;
			if (castle.IsAtProductionCapacity) continue;

			int currProduction = castle.QueuedUnitsCount;

			if (currProduction < minProduction)
			{
				minProduction = currProduction;
				minProductionCastle = castle;
			}
		}
		
		if (minProductionCastle == null) return null;

		List<uint> entities = new List<uint> { minProductionCastle.Id };
		EnqueueUnitProductionCommandArgs args = new EnqueueUnitProductionCommandArgs(entities, UnitType.Worker);
		return new EnqueueUnitProductionCommand(_playerId, args);
	}

	private ICommand? TrainKnight(WorldState state)
	{
		if (state.GetResource(_playerId, Resource.Gold) < BaseUnit.Dummy(UnitType.Knight).Cost.Amount) return null;

		IEnumerable<Barracks> playerBarracks = state.Structures.OfType<Barracks>().Where(c => c.OwnerId != _playerId);

		Barracks? minProductionBarracks = null;
		int minProduction = int.MaxValue;

		foreach (Barracks barracks in playerBarracks)
		{
			if (barracks.OwnerId != _playerId) continue;
			if (barracks.IsAtProductionCapacity) continue;

			int currProduction = barracks.QueuedUnitsCount;

			if (currProduction < minProduction)
			{
				minProduction = currProduction;
				minProductionBarracks = barracks;
			}
		}
		
		if (minProductionBarracks == null) return null;

		List<uint> entities = new List<uint> { minProductionBarracks.Id };
		EnqueueUnitProductionCommandArgs args = new EnqueueUnitProductionCommandArgs(entities, UnitType.Knight);
		return new EnqueueUnitProductionCommand(_playerId, args);
	}

	private ICommand? BuildBarracks(WorldState state)
	{
		Vec2Int center = new Vec2Int(_baseStart.Start.x, _baseStart.Start.y + Barracks.BaseHeight + 1);
		return BuildNew(state, StructureType.Barracks, center);
	}

	private ICommand? BuildCastle(WorldState state)
	{
		Vec2Int center = _baseStart.Start;
		return BuildNew(state, StructureType.Castle, center);
	}

	private ICommand? BuildNew(WorldState state, StructureType type, Vec2Int center)
	{
		BaseStructure dummy = BaseStructure.FromType(type, state, ~0u, Vec2Int.Zero);

		if (state.GetResource(_playerId, Resource.Gold) < dummy.Cost.Amount) return null;

		BaseStructure? structure = GetNextStructure(state, type, center);
		if (structure == null) return null;

		Worker? worker = GetMostAvailableWorker(state);
		if (worker == null) return null;

		List<uint> entities = new List<uint> { worker.Id };
		BuildNewCommandArgs args = new BuildNewCommandArgs(entities, structure.Start, type);
		return new BuildNewCommand(_playerId, args);
	}

	private BaseStructure? GetNextStructure(WorldState state, StructureType type, Vec2Int center)
	{
		BaseStructure dummy = BaseStructure.FromType(type, state, ~0u, Vec2Int.Zero);

		Grid<Cell> map = state.Map;
		int maxLength = Int32.Max((int)map.Width, (int)map.Height);
		maxLength = (maxLength / 2) + (maxLength % 2);

		Vec2Int offsets = new Vec2Int(dummy.Width + 1, dummy.Height + 1);
		Vec2Int minValue = center % offsets;
		// int yMin = center.y % offsets.y;
		// int xMin = center.x % offsets.x;

		for (int i = 0; i < maxLength; i++)
		{
			// Vec2Int currOffsets = offsets * i;
			Vec2Int currOffsets = new Vec2Int(offsets.x * i, offsets.y * (i - (i % 2)));

			Vec2Int start = center - currOffsets;
			Vec2Int end = center + currOffsets;

			int yStart = Int32.Max(start.y, minValue.y);
			int xStart = Int32.Max(start.x, minValue.x);

			bool hasOneEdge = false;

			if (start.x >= 0) // left edge is within map
			{
				hasOneEdge = true;
				for (int y = yStart; y < map.Height; y += 2 * offsets.y)
				{
					Vec2Int candidate = new Vec2Int(start.x, y);
					BaseStructure structure = BaseStructure.FromType(type, state, _playerId, candidate);
					if (!structure.IsAreaObstructed)
					{
						return structure;
					}
				}
			}

			if (end.x < map.Width) // right edge is within map
			{
				hasOneEdge = true;
				for (int y = yStart; y < map.Height; y += 2 * offsets.y)
				{
					Vec2Int candidate = new Vec2Int(end.x, y);
					BaseStructure structure = BaseStructure.FromType(type, state, _playerId, candidate);
					if (!structure.IsAreaObstructed)
					{
						return structure;
					}
				}
			}

			if (start.y >= 0) // down edge is within map
			{
				hasOneEdge = true;
				for (int x = xStart; x < map.Width; x += offsets.x)
				{
					Vec2Int candidate = new Vec2Int(x, start.y);
					BaseStructure structure = BaseStructure.FromType(type, state, _playerId, candidate);
					if (!structure.IsAreaObstructed)
					{
						return structure;
					}
				}
			}

			if (end.y < map.Height) // up edge is within map
			{
				hasOneEdge = true;
				for (int x = xStart; x < map.Width; x += offsets.x)
				{
					Vec2Int candidate = new Vec2Int(x, end.y);
					BaseStructure structure = BaseStructure.FromType(type, state, _playerId, candidate);
					if (!structure.IsAreaObstructed)
					{
						return structure;
					}
				}
			}

			if (!hasOneEdge) break;
		}

		return null;
	}

	private List<Worker> GetMostAvailableWorkers(WorldState state)
	{
		List<Worker> idleWorkers = new List<Worker>(50);
		List<Worker> defendingWorkers = new List<Worker>();
		List<Worker> gatheringWorkers = new List<Worker>();
		List<Worker> buildingWorkers = new List<Worker>();

		foreach (Worker worker in state.Units)
		{
			if (worker.OwnerId != _playerId) continue;
			Goal goal = worker.State.Goal;

			switch (goal)
			{
				case Goal.None:
					idleWorkers.Add(worker);
					break;
				case Goal.Walk:
					break; // attacker
				case Goal.Attack:
					defendingWorkers.Add(worker);
					break;
				case Goal.Gather:
					gatheringWorkers.Add(worker);
					break;
				case Goal.Build:
					buildingWorkers.Add(worker);
					break;
			}
		}

		idleWorkers.AddRange(gatheringWorkers);
		idleWorkers.AddRange(buildingWorkers);
		idleWorkers.AddRange(defendingWorkers);

		return idleWorkers;
	}

	private Worker? GetMostAvailableWorker(WorldState state)
	{
		Worker? defender = null;
		Worker? gathering = null;
		Worker? building = null;

		foreach (Worker worker in state.Units)
		{
			if (worker.OwnerId != _playerId) continue;
			Goal goal = worker.State.Goal;
			if (goal == Goal.None)
			{
				return worker;
			}
			else if (goal == Goal.Walk)
			{
				continue; // attacker
			}
			else if (goal == Goal.Attack)
			{
				defender = worker;
			}
			else if (goal == Goal.Gather)
			{
				gathering = worker;
			}
			else if (goal == Goal.Build)
			{
				building = worker;
			}
		}

		if (gathering != null) return gathering;
		if (building != null) return building;
		if (defender != null) return defender;
		return null;
	}

	private ICommand? Attack(RtsState state, WorldState world)
	{
		Vec2 enemyBase = state.EnemyBasePos;

		bool attackerIsKnight = false;
		BaseUnit? attacker = null;

		foreach (BaseUnit unit in world.Units)
		{
			if (unit.OwnerId != _playerId) continue;

			if (unit is Knight knight)
			{
				if (knight.State.Goal == Goal.None)
				{
					attackerIsKnight = true;
					attacker = unit;
					break;
				}
				else if (knight.State.Goal == Goal.Attack)
				{
					attackerIsKnight = true;
					attacker = unit;
				}
			}
			else if (!attackerIsKnight)
			{
				attacker = unit;
			}
		}

		if (attacker == null) return null;

		List<uint> entities = new List<uint> { attacker.Id };
		AggroMoveCommandArgs args = new AggroMoveCommandArgs(entities, enemyBase, true);
		return new AggroMoveCommand(_playerId, args);
	}

	private ICommand? Defend(RtsState state, WorldState world)
	{
		bool defenderIsKnight = false;
		BaseUnit? defender = null;

		BaseUnit? closestEnemy = null;
		float closestEnemyDistance = float.PositiveInfinity;
		int numTargets = int.MaxValue;

		foreach (BaseUnit unit in world.Units)
		{
			if (unit.OwnerId == _playerId)
			{
				if (defenderIsKnight) continue;
				else if (unit is Knight knight)
				{
					if (knight.State.Goal == Goal.Walk) continue; // is attacking
					else if (knight.State.Goal == Goal.Attack) continue; // is defending
					else if (knight.State.Goal == Goal.None)
					{
						defenderIsKnight = true;
					}
				}
				defender = unit;
				continue;
			}

			float distance = state.BasePos.Distance(unit.Pos);
			if (distance > state.BaseRadius) continue;

			if (unit.TargetedByNum > numTargets) continue;

			if (unit.TargetedByNum < numTargets)
			{
				closestEnemyDistance = distance;
				closestEnemy = unit;
				numTargets = unit.TargetedByNum;
				continue;
			}

			if (distance < closestEnemyDistance)
			{
				closestEnemyDistance = distance;
				closestEnemy = unit;
			}
		}

		if (defender == null) return null;
		if (closestEnemy == null) return null;

		List<uint> entities = new List<uint> { defender.Id };
		AttackCommandArgs args = new AttackCommandArgs(entities, closestEnemy.Id);
		return new AttackCommand(_playerId, args);
	}

	private ICommand? MineGold(WorldState state)
	{
		Dictionary<IGatherable, int> controlledNodes = new Dictionary<IGatherable, int>();
		HashSet<IGatherable> fullNodes = new HashSet<IGatherable>();

		foreach (Worker worker in state.Units)
		{
			if (worker.OwnerId != _playerId) continue;
			if (worker.State.Goal != Goal.Gather) continue;
			IGatherable? gatherable = worker.GatherableGoal;
			if (gatherable == null) continue;

			if (controlledNodes.ContainsKey(gatherable))
			{
				int workers = ++controlledNodes[gatherable];
				if (workers >= 3)
				{
					fullNodes.Add(gatherable);
				}
			}
			else
			{
				controlledNodes[gatherable] = 1;
			}
		}


		float minDistance = float.PositiveInfinity;
		GoldNode? nearestNode = null;

		foreach (GoldNode node in state.Entities)
		{
			if (fullNodes.Contains(node)) continue;

			float distance = node.Pos.Distance(state.Map.WorldSpaceFromCellPos(_baseStart.Start));
			if (distance < minDistance)
			{
				minDistance = distance;
				nearestNode = node;
			}
		}

		if (nearestNode == null) return null;
		Worker? availableWorker = GetMostAvailableWorker(state);
		if (availableWorker == null) return null;

		List<uint> entities = new List<uint> { availableWorker.Id };
		GatherCommandArgs args = new GatherCommandArgs(entities, nearestNode);
		return new GatherCommand(_playerId, args);
	}

	private ICommand? BuildUnfinishedStructure(WorldState state)
	{
		BaseStructure? unfinishedStructure = state.Structures.First(s => (s.OwnerId == _playerId) && !s.IsBuilt);
		if (unfinishedStructure == null) return null;
		Worker? worker = GetMostAvailableWorker(state);
		if (worker == null) return null;

		List<uint> entities = new List<uint> { worker.Id };
		BuildCommandArgs args = new BuildCommandArgs(entities, unfinishedStructure.Id);
		return new BuildCommand(_playerId, args);
	}

	private ICommand? Wait(WorldState state)
	{
		return null;
	}

}

}
