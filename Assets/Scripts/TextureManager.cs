using RtsEngine.EntityProperties;
using RtsEngine.Resources;
using RtsEngine.Structures;
using RtsEngine.Units;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
	[SerializeField]
	private WorldStateManager _worldStateManager = null!;

	public GameObject MissingTexture = null!;
	public GameObject[] WorkerPrefabs = null!;
	public GameObject[] KnightPrefabs = null!;
	public GameObject[] CastlePrefabs = null!;
	public GameObject[] BarracksPrefabs = null!;
	public GameObject[] GoldNodePrefabs = null!;

	public GameObject GetCorrespondingPrefab(Entity entity)
	{
		if (entity is Worker)
		{
			if (entity.OwnerId >= WorkerPrefabs.Length) return MissingTexture;
			return WorkerPrefabs[entity.OwnerId];
		}
		else if (entity is Knight)
		{
			if (entity.OwnerId >= KnightPrefabs.Length) return MissingTexture;
			return KnightPrefabs[entity.OwnerId];
		}
		else if (entity is Castle)
		{
			if (entity.OwnerId >= CastlePrefabs.Length) return MissingTexture;
			return CastlePrefabs[entity.OwnerId];
		}
		else if (entity is Barracks)
		{
			if (entity.OwnerId >= BarracksPrefabs.Length) return MissingTexture;
			return BarracksPrefabs[entity.OwnerId];
		}
		else if (entity is GoldNode goldNode)
		{
			if (GoldNodePrefabs.Length <= 0) return MissingTexture;
			return GoldNodePrefabs[0];
		}

		return MissingTexture;
	}

	public Vector3 GetInstanceCoordinates(Entity entity)
	{
		// if (entity is BaseUnit unit)
		// {
		// 	return new Vector3(unit.Pos.x, unit.Pos.y, unit.Pos.y);
		// }
		if (entity is IPositionable positionable)
		{
			return new Vector3(positionable.Pos.x, positionable.Pos.y, positionable.Pos.y);
		}
		else if (entity is BaseStructure structure)
		{
			float realStartY = _worldStateManager.LatestState!.Map.DownEdgeY(structure.Start);
			float realStartX = _worldStateManager.LatestState!.Map.LeftEdgeX(structure.Start);

			return new Vector3(realStartX, realStartY, realStartY);
		}

		return Vector3.zero;
	}
}
