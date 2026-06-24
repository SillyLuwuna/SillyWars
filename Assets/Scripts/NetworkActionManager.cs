using System.Collections.Generic;
using RtsEngine.Commands;
using RtsEngine.EntityProperties;
using RtsEngine.Math;
using RtsEngine.Structures;
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

	public void BuildNewAction(List<Entity> entities, Vec2 pos, StructureType type)
	{
		if (entities.Count == 0) return;

		Vec2Int start = _worldStateManager.LatestState.Map.CellPosFromWorldSpace(pos);

		List<uint> entityIds = GetSelectedEntityIds(entities);
		BuildNewCommandArgs args = new BuildNewCommandArgs(entityIds, start, type);
		ICommand command = new BuildNewCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	public void BuildAction(List<Entity> entities, Entity structure)
	{
		if (entities.Count == 0) return;

		List<uint> entityIds = GetSelectedEntityIds(entities);
		Debug.Log("Repairing!");
		BuildCommandArgs args = new BuildCommandArgs(entityIds, structure.Id);
		ICommand command = new BuildCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	public void MoveAction(List<Entity> entities, Vec2 goal)
	{
		if (entities.Count == 0) return;

		List<uint> entityIds = GetSelectedEntityIds(entities);
		MoveCommandArgs args = new MoveCommandArgs(entityIds, goal);
		ICommand command = new MoveCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	public void AttackAction(List<Entity> entities, Entity entity)
	{
		if (entities.Count == 0) return;

		List<uint> entityIds = GetSelectedEntityIds(entities);
		AttackCommandArgs args = new AttackCommandArgs(entityIds, entity.Id);
		ICommand command = new AttackCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	public void SetAggroAction(List<Entity> entities, bool aggro)
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
