using System;
using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using RtsEngine.Math;
using RtsEngine.Resources;

namespace RtsEngine.Structures
{

public abstract class BaseStructure : Entity, ISerializable, IDestroyable, IValuable
{
	public const int ConstructionMaxHitpoints = 10;

	private List<Vec2Int>? _surroundingTilesCache;

	public List<Vec2Int> Tiles = null!;

	public bool IsDestroyed { get; set; }
	public int TargetedByNum { get; set; }

	public abstract int Height { get; set; }
	public abstract int Width { get; set; }

	public abstract int MaxHitPoints { get; }
	public int HitPoints { get; set; }

	public abstract int BuildEffort { get; set; }
	public bool HasBuildingStarted;
	public bool IsBuilt { get; protected set; }

	public abstract ResourceStack Cost { get; }
	public bool IsPaid { get; set; }

	public Vec2Int Start;

	public BaseStructure(uint ownerId, Vec2Int start, int height, int width) : base(ownerId)
	{
		Init(start, height, width);
	}

	private void Init(Vec2Int start, int height, int width)
	{
		IsPaid = false;

		_surroundingTilesCache = null;
		Tiles = new List<Vec2Int>();
		Start = start;
		IsDestroyed = false;
		TargetedByNum = 0;
		IsBuilt = false;
		HitPoints = ConstructionMaxHitpoints;

		Height = height;
		Width = width;

		InitializeStructureTiles();
	}

	private void InitializeStructureTiles()
	{
		for (int x = 0; x < Width; x++)
		{
			for (int y = 0; y < Height; y++)
			{
				Tiles.Add(new Vec2Int(Start.x + x, Start.y + y));
			}
		}
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);

		writer.Write(Start);
		writer.Write(Height);
		writer.Write(Width);

		writer.Write(IsDestroyed);

		writer.Write(HitPoints);

		writer.Write(BuildEffort);
		writer.Write(HasBuildingStarted);
		writer.Write(IsBuilt);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);

		Start = reader.Read<Vec2Int>();
		Height = reader.Read<int>();
		Width = reader.Read<int>();
		Init(Start, Height, Width);

		IsDestroyed = reader.Read<bool>();

		HitPoints = reader.Read<int>();

		BuildEffort = reader.Read<int>();
		HasBuildingStarted = reader.Read<bool>();
		IsBuilt = reader.Read<bool>();
	}

	public void StartBuilding()
	{
		if (IsAreaObstructed) return;

		HasBuildingStarted = true;
		RtsEngine.Instance.State.AddEntity(this);
	}

	public void DoBuildWork()
	{
		if (IsBuilt)
		{
			HitPoints = System.Math.Min(HitPoints + 1, MaxHitPoints);
			return;
		}

		if (!HasBuildingStarted) return;

		BuildEffort--;

		if (BuildEffort <= 0)
		{
			IsBuilt = true;
			HitPoints = MaxHitPoints;
		}
	}

	public bool IsAreaObstructed
	{
		get
		{
			WorldState state = RtsEngine.Instance.State;
			foreach (Vec2Int tile in Tiles)
			{
				if (state.IsTileOccupied(tile)) return true;
			}
			return false;
		}
	}

	public bool IsFullyBuilt => IsBuilt && HasMaxHP;

	public bool HasMaxHP => HitPoints == MaxHitPoints;

	private List<Vec2Int> GetSurroundingTiles()
	{
		List<Vec2Int> surroundingTiles = new List<Vec2Int>();

		for (int x = 0; x < Width ; x++)
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

	public List<Vec2Int> SurroundingTiles {
		get
		{
			if (_surroundingTilesCache == null)
			{
				_surroundingTilesCache = GetSurroundingTiles();
			}

			return _surroundingTilesCache;
		}
	}

	public static BaseStructure FromType(StructureType type, uint ownerId, Vec2Int start)
	{
		switch (type)
		{
			case (StructureType.Castle):
				return new Castle(ownerId, start);
			case (StructureType.Barracks):
				return new Barracks(ownerId, start);
			default:
				throw new ArgumentException($"Unknown structure type {type}");
		}
	}

	public abstract StructureType StructureType { get; }
}

}
