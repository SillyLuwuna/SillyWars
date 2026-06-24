using System.Collections.Generic;
using RtsEngine.Commands;
using RtsEngine.EntityProperties;
using RtsEngine.Math;
using RtsEngine.Structures;
using RtsEngine.Units;
using UnityEngine;

public class NetworkActionManager : MonoBehaviour
{
	// should validate actions not to overload server

	[SerializeField]
	private WorldStateManager _worldStateManager = null!;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

	public void SetProductionSpawn(List<Entity> entities, Vec2 spawnpoint)
	{
		if (entities.Count == 0) return;

		List<uint> entityIds = GetSelectedEntityIds(entities);
		SetProductionSpawnpointCommandArgs args = new SetProductionSpawnpointCommandArgs(entityIds, spawnpoint);
		ICommand command = new SetProductionSpawnCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	public void EnqueueUnitProduction(List<Entity> entities, UnitType type)
	{
		if (entities.Count == 0) return;

		List<uint> entityIds = GetSelectedEntityIds(entities);
		EnqueueUnitProductionCommandArgs args = new EnqueueUnitProductionCommandArgs(entityIds, type);
		ICommand command = new EnqueueUnitProductionCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	public void BuildNew(List<Entity> entities, Vec2 pos, StructureType type)
	{
		if (entities.Count == 0) return;

		Vec2Int start = _worldStateManager.LatestState.Map.CellPosFromWorldSpace(pos);

		List<uint> entityIds = GetSelectedEntityIds(entities);
		BuildNewCommandArgs args = new BuildNewCommandArgs(entityIds, start, type);
		ICommand command = new BuildNewCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	public void Build(List<Entity> entities, Entity structure)
	{
		if (entities.Count == 0) return;

		List<uint> entityIds = GetSelectedEntityIds(entities);
		Debug.Log("Repairing!");
		BuildCommandArgs args = new BuildCommandArgs(entityIds, structure.Id);
		ICommand command = new BuildCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	public void Move(List<Entity> entities, Vec2 goal)
	{
		if (entities.Count == 0) return;

		List<uint> entityIds = GetSelectedEntityIds(entities);
		MoveCommandArgs args = new MoveCommandArgs(entityIds, goal);
		ICommand command = new MoveCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	public void Attack(List<Entity> entities, Entity entity)
	{
		if (entities.Count == 0) return;

		List<uint> entityIds = GetSelectedEntityIds(entities);
		AttackCommandArgs args = new AttackCommandArgs(entityIds, entity.Id);
		ICommand command = new AttackCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	public void SetAggro(List<Entity> entities, bool aggro)
	{
		if (entities.Count == 0) return;

		List<uint> entityIds = GetSelectedEntityIds(entities);
		SetAggroCommandArgs args = new SetAggroCommandArgs(entityIds, aggro);
		ICommand command = new SetAggroCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	private List<uint> GetSelectedEntityIds(List<Entity> entities)
	{
		List<uint> entityIds = new List<uint>();

		foreach (Entity entity in entities)
		{
			entityIds.Add(entity.Id);
		}

		return entityIds;
	}
}
