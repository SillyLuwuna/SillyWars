using RtsEngine.EntityProperties;
using RtsEngine.Math;
using RtsEngine.Resources;
using RtsEngine.Structures;
using RtsEngine.Units;

namespace RtsEngine.AI
{

public enum StateEntry
{
	Gold,
	GoldIncome,

	IdleWorkers,
	Workers,
	Knights,
	TotalUnits,

	EnemyWorkers,
	EnemyKnights,
	TotalEnemyUnits,

	ArmyRatio,

	EnqueuedWorkers,
	EnqueuedKnights,

	Castles,
	Barracks,

	EnemyCastles,
	EnemyBarracks,

	ClosestEnemyStructureDistance,

	NearestUnworkedGoldNodeDistance,
	GoldNodesControlledRatio,

	EnemyArmyValueNearBase,
	ArmyValueNearEnemyBase,

	GameTime
}

public struct RtsState
{
	public static readonly int Size = Enum.GetValues(typeof(StateEntry)).Length;

	const float workerArmyValue = (float)Worker.BaseAttackDamage / (float)Worker.BaseAttackSpeed;
	const float knightArmyValue = (float)Knight.BaseAttackDamage / (float)Knight.BaseAttackSpeed;
	const float maxArmyValue = 50f * knightArmyValue;

	private float[] _state;

	private uint _playerId;
	private Vec2 _basePos;
	private float _baseRadius;

	private uint _enemyId;
	private Vec2 _enemyBasePos;
	private float _enemyBaseRadius;

	public RtsState(RtsState? previousState, WorldState state, uint playerId, ulong elapsedTicks)
	{
		_state = new float[Size];
		_enemyId = playerId == 0 ? 1u : 0u;
		_basePos = GetBasePos(state, playerId);
		_baseRadius = GetBaseRadius(_basePos, state, playerId);
		_enemyBasePos = GetBasePos(state, _enemyId);
		_enemyBaseRadius = GetBaseRadius(_enemyBasePos, state, _enemyId);

		SetGoldInfo(previousState, state);
		SetUnitInfo(state);
		SetStructureInfo(state);
		SetClosestEnemyStructure(state);
		SetGoldNodeInfo(state);
		SetBaseArmyValues(state);
		SetGameTime(elapsedTicks);
	}

	private Vec2 GetBasePos(WorldState worldState, uint playerId)
	{
		Vec2 centroid = Vec2.Zero;
		int count = 0;

		foreach (BaseStructure structure in worldState.Structures)
		{
			if (structure.OwnerId == playerId)
			{
				centroid.x += worldState.Map.LeftEdgeX(structure.Start) + ((float)structure.Width / 2f);
				centroid.y += worldState.Map.DownEdgeY(structure.Start) + ((float)structure.Height / 2f);

				count++;
			}
		}
		
		return centroid / (float)count;
	}

	private float GetBaseRadius(Vec2 basePos, WorldState worldState, uint playerId)
	{
		float maxDistance = 0.0f;

		foreach (BaseStructure structure in worldState.Structures)
		{
			if (structure.OwnerId != playerId) continue;

			Vec2 pos = worldState.Map.WorldSpaceFromCellPos(structure.Start);
			float distance = basePos.Distance(pos);

			if (distance > maxDistance)
			{
				maxDistance = distance;
			}
		}

		return maxDistance + 5.0f;
	}

	private void SetGoldInfo(RtsState? previousState, WorldState worldState)
	{
		Set(StateEntry.Gold, worldState.GetResource(_playerId, Resource.Gold) / 1000f);
		Set(StateEntry.GoldIncome, previousState == null ? 0 : previousState.Value.Get(StateEntry.Gold) / 50f);
	}

	private void SetUnitInfo(WorldState worldState)
	{
		int idleWorkerCount = 0;

		int workerCount = 0;
		int enemyWorkerCount = 0;

		int knightCount = 0;
		int enemyKnightCount = 0;

		int totalUnits = 0;
		int totalEnemyUnits = 0;

		foreach (BaseUnit unit in worldState.Units)
		{
			if (unit is Worker worker)
			{
				if (unit.OwnerId == _playerId)
				{
					if (worker.State.Goal == Goal.None)
					{
						idleWorkerCount++;
					}
					workerCount++;
					totalUnits++;
				}
				else
				{
					enemyWorkerCount++;
					totalEnemyUnits++;
				}
			}
			else if (unit is Knight)
			{
				if (unit.OwnerId == _playerId)
				{
					knightCount++;
					totalUnits++;
				}
				else
				{
					enemyKnightCount++;
					totalEnemyUnits++;
				}
			}
		}

		Set(StateEntry.IdleWorkers, idleWorkerCount / 50f);
		Set(StateEntry.Workers, workerCount / 50f);
		Set(StateEntry.Knights, knightCount / 50f);
		Set(StateEntry.TotalUnits, totalUnits / 50f);

		Set(StateEntry.EnemyWorkers, enemyWorkerCount / 50f);
		Set(StateEntry.EnemyKnights, enemyKnightCount / 50f);
		Set(StateEntry.TotalEnemyUnits, totalEnemyUnits / 50f);

		float armyValue = workerCount * workerArmyValue + knightCount * knightArmyValue;
		float enemyArmyValue = enemyWorkerCount * workerArmyValue + enemyKnightCount * knightArmyValue;

		float armyRatio;
		if (totalEnemyUnits == 0)
		{
			armyRatio = maxArmyValue;
		}
		else
		{
			armyRatio = MathF.Min((armyValue) / (enemyArmyValue), maxArmyValue);
		}
		Set(StateEntry.ArmyRatio, armyRatio / maxArmyValue);
	}

	private void SetStructureInfo(WorldState worldState)
	{
		int enqueuedWorkerCount = 0;
		int enqueuedKnightCount = 0;

		int barracksCount = 0;
		int enemyBarracksCount = 0;
		int castleCount = 0;
		int enemyCastleCount = 0;

		foreach (BaseStructure structure in worldState.Structures)
		{
			if (structure is Barracks)
			{
				if (structure.OwnerId == _playerId)
				{
					barracksCount++;
				}
				else
				{
					enemyBarracksCount++;
				}
			}
			else if (structure is Castle)
			{
				if (structure.OwnerId == _playerId)
				{
					castleCount++;
				}
				else
				{
					enemyCastleCount++;
				}
			}

			if (structure is UnitProducer producer)
			{
				if (producer.OwnerId != _playerId) continue;
				if (!producer.IsProducingUnits) continue;

				UnitType head = producer.ProductionQueueHead!.Value;
				Queue<UnitType> productionQueue = producer.ProductionQueue;
				productionQueue.Enqueue(head);

				foreach (UnitType unit in productionQueue)
				{
					if (unit == UnitType.Worker)
					{
						enqueuedWorkerCount++;
					}
					else if (unit == UnitType.Knight)
					{
						enqueuedKnightCount++;
					}
				}
			}
		}

		Set(StateEntry.EnqueuedWorkers, enqueuedWorkerCount / 50f);
		Set(StateEntry.EnqueuedKnights, enqueuedKnightCount / 50f);

		Set(StateEntry.Castles, castleCount / 20f);
		Set(StateEntry.Barracks, barracksCount / 20f);

		Set(StateEntry.EnemyCastles, enemyCastleCount / 20f);
		Set(StateEntry.EnemyBarracks, enemyBarracksCount / 20f);
	}

	private void SetClosestEnemyStructure(WorldState worldState)
	{
		float maxDistance = Vec2.Zero.Distance(new Vec2(worldState.Map.Width, worldState.Map.Height));

		BaseStructure? closestStructure = null;
		float closestDistance = maxDistance;
		foreach (BaseStructure structure in worldState.Structures)
		{
			if (structure.OwnerId != _playerId)
			{
				Vec2 pos = worldState.Map.WorldSpaceFromCellPos(structure.Start); 
				float distance = _basePos.Distance(pos);

				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestStructure = structure;
				}
			}
		}

		Set(StateEntry.ClosestEnemyStructureDistance, closestDistance / maxDistance);
	}

	private void SetGoldNodeInfo(WorldState worldState)
	{
		HashSet<IGatherable> controlledNodes = new HashSet<IGatherable>();

		foreach (Worker worker in worldState.Units)
		{
			if (worker.OwnerId != _playerId) continue;
			if (worker.State.Goal != Goal.Gather) continue;
			IGatherable? gatherable = worker.GatherableGoal;
			if (gatherable == null) continue;

			controlledNodes.Add(gatherable);
		}

		float maxDistance = Vec2.Zero.Distance(new Vec2(worldState.Map.Width, worldState.Map.Height));

		int totalGoldNodes = 0;
		float closestUnclaimedNodeDistance = maxDistance;
		foreach (GoldNode node in worldState.Entities)
		{
			totalGoldNodes++;
			if (!controlledNodes.Contains(node))
			{
				closestUnclaimedNodeDistance = _basePos.Distance(node.Pos);
			}
		}

		Set(StateEntry.NearestUnworkedGoldNodeDistance, closestUnclaimedNodeDistance / maxDistance);
		Set(StateEntry.GoldNodesControlledRatio, (float)controlledNodes.Count / (float)totalGoldNodes);
	}

	private void SetBaseArmyValues(WorldState worldState)
	{
		float enemyArmyValueNearBase = ArmyValueOfUnitsInBase(_basePos, _baseRadius, worldState, _enemyId);
		float armyValueNearEnemyBase = ArmyValueOfUnitsInBase(_enemyBasePos, _enemyBaseRadius, worldState, _playerId);

		Set(StateEntry.EnemyArmyValueNearBase, enemyArmyValueNearBase / maxArmyValue);
		Set(StateEntry.ArmyValueNearEnemyBase, armyValueNearEnemyBase / maxArmyValue);
	}

	public static float ArmyValueOfUnitsInBase(Vec2 basePos, float baseRadius, WorldState worldState, uint playerIdUnitsToCheck)
	{
		float armyValue = 0.0f;

		foreach (BaseUnit unit in worldState.Units)
		{
			if (unit.OwnerId != playerIdUnitsToCheck) continue;
			if (basePos.Distance(unit.Pos) > baseRadius) continue;

			if (unit is Worker)
			{
				armyValue += workerArmyValue;
			}
			else if (unit is Knight)
			{
				armyValue += knightArmyValue;
			}
		}

		return armyValue;
	}

	public void SetGameTime(ulong elapsedTicks)
	{
		Set(StateEntry.GameTime, (float)elapsedTicks / (float)Trainer.MaxAllowedTicks);
	}

	private void Set(StateEntry entry, float value)
	{
		_state[(int)entry] = value;
	}

	private float Get(StateEntry entry)
	{
		return _state[(int)entry];
	}

	private float Gold
	{
		get => _state[0];
		set => _state[0] = value;
	}

	public float[] Array => _state;
}

}
