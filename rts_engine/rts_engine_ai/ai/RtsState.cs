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

	// idle knights
	// defending knights

	// workersBuilding
	// unfinishedStructures

	// last command

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
	private float[] _values;

	public uint PlayerId;
	public Vec2 BasePos;
	public float BaseRadius;

	public uint EnemyId;
	public Vec2 EnemyBasePos;
	public float EnemyBaseRadius;

	public RtsState(RtsState? previousState, WorldState state, uint playerId, ulong elapsedTicks)
	{
		_state = new float[Size];
		_values = new float[Size];
		EnemyId = playerId == 0 ? 1u : 0u;
		BasePos = GetBasePos(previousState == null ? null : previousState.Value.BasePos, state, playerId);
		BaseRadius = GetBaseRadius(BasePos, state, playerId);
		EnemyBasePos = GetBasePos(previousState == null ? null : previousState.Value.EnemyBasePos, state, EnemyId);
		EnemyBaseRadius = GetBaseRadius(EnemyBasePos, state, EnemyId);

		SetGoldInfo(previousState, state);
		SetUnitInfo(state);
		SetStructureInfo(state);
		SetClosestEnemyStructure(state);
		SetGoldNodeInfo(state);
		SetBaseArmyValues(state);
		SetGameTime(elapsedTicks);
	}

	private Vec2 GetBasePos(Vec2? previousPos, WorldState worldState, uint playerId)
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

		if (count == 0)
		{
			if (previousPos == null) throw new ArgumentException("Invalid state: no structures nor previous state");

			return previousPos.Value;
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
		float currGold = worldState.GetResource(PlayerId, Resource.Gold);
		SetValue(StateEntry.Gold, currGold);
		Set(StateEntry.Gold, currGold / 1000f);

		float lastGold = previousState == null ? 0 : previousState.Value.GetValue(StateEntry.Gold);
		float goldDifference = currGold - lastGold;
		float goldDifferenceNormalized = 0.5f + (goldDifference / 50f / 2f);
		SetValue(StateEntry.GoldIncome, previousState == null ? 0 : goldDifference);
		Set(StateEntry.GoldIncome, previousState == null ? 0 : goldDifferenceNormalized);
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
				if (unit.OwnerId == PlayerId)
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
				if (unit.OwnerId == PlayerId)
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



		SetValue(StateEntry.IdleWorkers, idleWorkerCount);
		Set(StateEntry.IdleWorkers, idleWorkerCount / 50f);
		SetValue(StateEntry.Workers, workerCount);
		Set(StateEntry.Workers, workerCount / 50f);
		SetValue(StateEntry.Knights, knightCount);
		Set(StateEntry.Knights, knightCount / 50f);
		SetValue(StateEntry.TotalUnits, totalUnits);
		Set(StateEntry.TotalUnits, totalUnits / 50f);

		SetValue(StateEntry.EnemyWorkers, enemyWorkerCount);
		Set(StateEntry.EnemyWorkers, enemyWorkerCount / 50f);
		SetValue(StateEntry.EnemyKnights, enemyKnightCount);
		Set(StateEntry.EnemyKnights, enemyKnightCount / 50f);
		SetValue(StateEntry.TotalEnemyUnits, totalEnemyUnits);
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
		SetValue(StateEntry.ArmyRatio, armyRatio);
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
				if (structure.OwnerId == PlayerId)
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
				if (structure.OwnerId == PlayerId)
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
				if (producer.OwnerId != PlayerId) continue;
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

		SetValue(StateEntry.EnqueuedWorkers, enqueuedWorkerCount);
		Set(StateEntry.EnqueuedWorkers, enqueuedWorkerCount / 50f);
		SetValue(StateEntry.EnqueuedKnights, enqueuedKnightCount);
		Set(StateEntry.EnqueuedKnights, enqueuedKnightCount / 50f);

		SetValue(StateEntry.Castles, castleCount);
		Set(StateEntry.Castles, castleCount / 20f);
		SetValue(StateEntry.Barracks, barracksCount);
		Set(StateEntry.Barracks, barracksCount / 20f);

		SetValue(StateEntry.EnemyCastles, enemyCastleCount);
		Set(StateEntry.EnemyCastles, enemyCastleCount / 20f);
		SetValue(StateEntry.EnemyBarracks, enemyBarracksCount);
		Set(StateEntry.EnemyBarracks, enemyBarracksCount / 20f);
	}

	private void SetClosestEnemyStructure(WorldState worldState)
	{
		float maxDistance = Vec2.Zero.Distance(new Vec2(worldState.Map.Width, worldState.Map.Height));

		BaseStructure? closestStructure = null;
		float closestDistance = maxDistance;
		foreach (BaseStructure structure in worldState.Structures)
		{
			if (structure.OwnerId != PlayerId)
			{
				Vec2 pos = worldState.Map.WorldSpaceFromCellPos(structure.Start); 
				float distance = BasePos.Distance(pos);

				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestStructure = structure;
				}
			}
		}

		SetValue(StateEntry.ClosestEnemyStructureDistance, closestDistance);
		Set(StateEntry.ClosestEnemyStructureDistance, closestDistance / maxDistance);
	}

	private void SetGoldNodeInfo(WorldState worldState)
	{
		HashSet<IGatherable> controlledNodes = new HashSet<IGatherable>();

		foreach (Worker worker in worldState.Units)
		{
			if (worker.OwnerId != PlayerId) continue;
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
				closestUnclaimedNodeDistance = BasePos.Distance(node.Pos);
			}
		}

		SetValue(StateEntry.NearestUnworkedGoldNodeDistance, closestUnclaimedNodeDistance);
		Set(StateEntry.NearestUnworkedGoldNodeDistance, closestUnclaimedNodeDistance / maxDistance);
		SetValue(StateEntry.GoldNodesControlledRatio, (float)controlledNodes.Count / (float)totalGoldNodes);
		Set(StateEntry.GoldNodesControlledRatio, (float)controlledNodes.Count / (float)totalGoldNodes);
	}

	private void SetBaseArmyValues(WorldState worldState)
	{
		float enemyArmyValueNearBase = ArmyValueOfUnitsInBase(BasePos, BaseRadius, worldState, EnemyId);
		float armyValueNearEnemyBase = ArmyValueOfUnitsInBase(EnemyBasePos, EnemyBaseRadius, worldState, PlayerId);

		SetValue(StateEntry.EnemyArmyValueNearBase, enemyArmyValueNearBase);
		Set(StateEntry.EnemyArmyValueNearBase, enemyArmyValueNearBase / maxArmyValue);
		SetValue(StateEntry.ArmyValueNearEnemyBase, armyValueNearEnemyBase);
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
		SetValue(StateEntry.GameTime, (float)elapsedTicks);
		Set(StateEntry.GameTime, (float)elapsedTicks / (float)Trainer.MaxAllowedTicks);
	}

	private void Set(StateEntry entry, float value)
	{
		_state[(int)entry] = value;
	}

	private void SetValue(StateEntry entry, float value)
	{
		_values[(int)entry] = value;
	}

	public float GetValue(StateEntry entry)
	{
		return _values[(int)entry];
	}

	public float Gold => this.GetValue(StateEntry.Gold);
	public float GoldIncome => this.GetValue(StateEntry.GoldIncome);
	public float IdleWorkers => this.GetValue(StateEntry.IdleWorkers);
	public float Workers => this.GetValue(StateEntry.Workers);
	public float Knights => this.GetValue(StateEntry.Knights);
	public float TotalUnits => this.GetValue(StateEntry.TotalUnits);
	// public float IdleKnights => this.Get(StateEntry.IdleKnights);
	// public float DefendingKnights => this.Get(StateEntry.DefendingKnights);
	// public float WorkersBuilding => this.Get(StateEntry.WorkersBuilding);
	// public float UnfinishedStructures => this.Get(StateEntry.UnfinishedStructures);
	// public float LastCommand => this.Get(StateEntry.LastCommand);
	public float EnemyWorkers => this.GetValue(StateEntry.EnemyWorkers);
	public float EnemyKnights => this.GetValue(StateEntry.EnemyKnights);
	public float TotalEnemyUnits => this.GetValue(StateEntry.TotalEnemyUnits);
	public float ArmyRatio => this.GetValue(StateEntry.ArmyRatio);
	public float EnqueuedWorkers => this.GetValue(StateEntry.EnqueuedWorkers);
	public float EnqueuedKnights => this.GetValue(StateEntry.EnqueuedKnights);
	public float Castles => this.GetValue(StateEntry.Castles);
	public float Barracks => this.GetValue(StateEntry.Barracks);
	public float EnemyCastles => this.GetValue(StateEntry.EnemyCastles);
	public float EnemyBarracks => this.GetValue(StateEntry.EnemyBarracks);
	public float ClosestEnemyStructureDistance => this.GetValue(StateEntry.ClosestEnemyStructureDistance);
	public float NearestUnworkedGoldNodeDistance => this.GetValue(StateEntry.NearestUnworkedGoldNodeDistance);
	public float GoldNodesControlledRatio => this.GetValue(StateEntry.GoldNodesControlledRatio);
	public float EnemyArmyValueNearBase => this.GetValue(StateEntry.EnemyArmyValueNearBase);
	public float ArmyValueNearEnemyBase => this.GetValue(StateEntry.ArmyValueNearEnemyBase);
	public float GameTime;

	public float[] Array => _state;
}

}
