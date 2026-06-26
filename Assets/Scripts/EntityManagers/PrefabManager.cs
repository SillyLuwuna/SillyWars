using RtsEngine.EntityProperties;
using RtsEngine.Resources;
using RtsEngine.Structures;
using RtsEngine.Units;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
	public GameObject UnitPrefab = null!;
	public GameObject StructurePrefab = null!;
	public GameObject ResourceNodePrefab = null!;
	public GameObject UnknownPrefab = null!;

	public GameObject GetCorrespondingPrefab(Entity entity)
	{
		if (entity is Worker)
		{
			return UnitPrefab;
		}
		else if (entity is Knight)
		{
			return UnitPrefab;
		}
		else if (entity is Castle)
		{
			return StructurePrefab;
		}
		else if (entity is Barracks)
		{
			return StructurePrefab;
		}
		else if (entity is GoldNode goldNode)
		{
			return ResourceNodePrefab;
		}

		return UnknownPrefab;
	}

	public Vector3 GetInstanceCoordinates(Entity entity)
	{
		if (entity is IPositionable positionable)
		{
			return new Vector3(positionable.Pos.x, positionable.Pos.y, positionable.Pos.y);
		}
		else if (entity is BaseStructure structure)
		{
			float realStartY = WorldStateManager.Instance.LatestState!.Map.DownEdgeY(structure.Start);
			float realStartX = WorldStateManager.Instance.LatestState!.Map.LeftEdgeX(structure.Start);

			return new Vector3(realStartX, realStartY, realStartY);
		}

		return Vector3.zero;
	}
}
