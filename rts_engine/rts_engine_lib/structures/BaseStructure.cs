using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using RtsEngine.Math;

namespace RtsEngine.Structures
{

public abstract class BaseStructure : Entity, ISerializable, IDestroyable
{
	public List<Vec2Int> StructureTiles;

	public bool IsDestroyed { get; set; }

	public abstract int Height { get; set; }
	public abstract int Width { get; set; }

	public abstract int MaxHitPoints { get; set; }
	public int HitPoints { get; set; }
	public const int CONSTRUCTION_MAX_HITPOINTS = 10;

	public abstract int BuildEffort { get; set; }
	public bool HasBuildingStarted;
	public bool IsBuilt { get; protected set; }

	public Vec2Int Start;

	public BaseStructure(uint ownerId, Vec2Int start) : base(ownerId)
	{
		StructureTiles = new List<Vec2Int>();
		Start = start;
		IsDestroyed = false;
		IsBuilt = false;
		HitPoints = CONSTRUCTION_MAX_HITPOINTS;

		InitializeStructureCells();
	}

	private void InitializeStructureCells()
	{
		for (int x = 0; x < Width; x++)
		{
			for (int y = 0; y < Height; y++)
			{
				StructureTiles.Add(new Vec2Int(Start.x + x, Start.y + y));
			}
		}
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
	}

	public void StartBuilding()
	{
		HasBuildingStarted = true;
	}

	public void DoBuildWork()
	{
		if (IsBuilt)
		{
			HitPoints = System.Math.Min(HitPoints + 1, MaxHitPoints);
			return;
		}

		if (HasBuildingStarted) return;

		BuildEffort--;

		if (BuildEffort <= 0)
		{
			IsBuilt = true;
			HitPoints = MaxHitPoints;
		}
	}

	public bool IsStructureAreaObstructed
	{
		get
		{
			WorldState state = RtsEngine.Instance.State;
			foreach (Vec2Int tile in StructureTiles)
			{
				if (state.IsTileOccupied(tile)) return false;
			}
			return true;
		}
	}

	public bool IsFullyBuilt { get => IsBuilt && HasMaxHP; }

	public bool HasMaxHP { get => HitPoints == MaxHitPoints; }

	public List<Vec2Int> GetSurroundingTiles()
	{
		List<Vec2Int> surroundingTiles = new List<Vec2Int>();

		for (int x = -1; x < Width + 1; x++)
		{
			surroundingTiles.Add(new Vec2Int(Start.x + x, Start.y - 1));
			surroundingTiles.Add(new Vec2Int(Start.x + x, Start.y + Height));
		}

		for (int y = 0; y < Height; y++)
		{
			surroundingTiles.Add(new Vec2Int(Start.x - 1, Start.y + y));
			surroundingTiles.Add(new Vec2Int(Start.x + Width, Start.y + y));
		}

		return surroundingTiles;
	}
}

}
