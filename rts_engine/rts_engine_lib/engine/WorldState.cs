using RtsEngine.Data;
using RtsEngine.Units;
using RtsEngine.Map;
using System.IO.Compression;

using System.IO;
using System.Collections.Generic;
using RtsEngine.EntityProperties;
using System;
using RtsEngine.Physics;
using RtsEngine.Math;
using RtsEngine.Structures;

namespace RtsEngine
{

public class WorldState : ISerializable
{
	private const bool DEBUG = true;

	public Grid<Cell> Map = null!;
	private Dictionary<uint, Entity> _entitiesId = null!;
	private List<Entity> _entities = null!;
	private List<BaseUnit> _units = null!;
	private List<PhysicsObject> _physicsObjects = null!;
	private List<IDestroyable> _destroyables = null!;
	private List<BaseStructure> _structures = null!;

	private Queue<Entity> _addQueue = null!;
	private bool _isTickingEntities;

	private PathFinder _pathFinder = null!;

	public int PlayerVersion { get; private set; }

	public WorldState(Grid<Cell> map)
	{
		Init(map);
	}

	private void Init(Grid<Cell> map)
	{
		Map = map;
		_entitiesId = new Dictionary<uint, Entity>();
		_entities = new List<Entity>();
		_units = new List<BaseUnit>();
		_physicsObjects = new List<PhysicsObject>();
		_destroyables = new List<IDestroyable>();
		_structures = new List<BaseStructure>();

		_addQueue = new Queue<Entity>();
		_isTickingEntities = false;

		_pathFinder = new PathFinder(Map);
	}

	public void SerializeFields(SerializerWriter writer)
	{
		writer.Write(PlayerVersion);

		writer.Write(Map);

		writer.Write(_entities.Count);
		foreach (Entity entity in _entities)
		{
			writer.Write(entity);
		}

	}

	public void DeserializeFields(SerializerReader reader)
	{
		PlayerVersion = reader.Read<int>();

		Grid<Cell> map = reader.Read<Grid<Cell>>();
		Init(map);

		int entitiesNum = reader.Read<int>();
		for (int i = 0; i < entitiesNum; i++)
		{
			Entity curr = reader.Read<Entity>();
			AddEntity(curr);
		}
	}

	public void AddEntity(Entity entity)
	{
		if (_isTickingEntities)
		{
			_addQueue.Enqueue(entity);
			return;
		}

		_entities.Add(entity);
		_entitiesId.Add(entity.Id, entity);

		if (entity is BaseUnit currUnit)
		{
			_units.Add(currUnit);
		}

		if (entity is PhysicsObject currPhysics)
		{
			_physicsObjects.Add(currPhysics);
		}

		if (entity is IDestroyable currDestroyable)
		{
			_destroyables.Add(currDestroyable);
		}

		if (entity is BaseStructure currStructure)
		{
			_structures.Add(currStructure);
			UpdateMapStructure(currStructure);
		}
	}

	private void UpdateMapStructure(BaseStructure structure)
	{
		CellType cellType = structure.IsDestroyed ? CellType.Ground : CellType.Structure;

		foreach (Vec2Int tile in structure.Tiles)
		{
			Map[tile].Type = cellType;
		}
	}

	public void Tick()
	{
		TickEntities();
		AddQueuedEntities();
	}

	private void TickEntities()
	{
		_isTickingEntities = true;

		int numEntities = _entities.Count;
		for (int i = 0; i < numEntities; i++)
		{
			try
			{
				_entities[i].Tick();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error when ticking entity {_entities[i].Id}: {ex.Message}");
				if (DEBUG)
				{
					#pragma warning disable CS0162
					Console.WriteLine(ex.StackTrace);
					#pragma warning restore CS0162
				}
			}
		}

		_isTickingEntities = false;
	}

	private void AddQueuedEntities()
	{
		while (_addQueue.Count > 0)
		{
			AddEntity(_addQueue.Dequeue());
		}
	}

	public Entity? GetEntity(uint id)
	{
		if (!_entitiesId.ContainsKey(id)) return null;

		return _entitiesId[id];
	}

	public void CleanupDestroyedEntities()
	{
		List<uint> destroyedEntities = GetDestroyedEntities();

		foreach (uint entityId in destroyedEntities)
		{
			RemoveEntity(entityId);
		}
	}

	public List<uint> GetDestroyedEntities()
	{
		List<uint> destroyedEntities = new List<uint>();

		for (uint i = 0; i < _destroyables.Count; i++)
		{
			IDestroyable destroyable = _destroyables[(int)i];
			if (destroyable.IsDestroyed)
			{
				destroyedEntities.Add(((Entity)destroyable).Id);
			}
		}

		return destroyedEntities;
	}

	public void RemoveEntity(uint entityId)
	{
		Entity? entity = GetEntity(entityId);
		if (entity == null) return;
		RemoveEntity(entity);
	}

	public void RemoveEntity(Entity entity)
	{
		// can be optimized by lazy deleting from arrays
		_entities.Remove(entity);
		_entitiesId.Remove(entity.Id);
		if (entity is BaseUnit currUnit)
		{
			_units.Remove(currUnit);
		}

		if (entity is PhysicsObject currPhysics)
		{
			_physicsObjects.Remove(currPhysics);
		}

		if (entity is IDestroyable currDestroyable)
		{
			_destroyables.Remove(currDestroyable);
		}

		if (entity is BaseStructure currStructure)
		{
			_structures.Remove(currStructure);
			UpdateMapStructure(currStructure);
		}
	}

	public List<BaseUnit> Units { get => _units; }

	public List<BaseStructure> Structures { get => _structures; }

	public List<PhysicsObject> PhysicsObjects { get => _physicsObjects; }

	public PathFinder PathFinder { get => _pathFinder; }

	public bool IsTileOccupied(Vec2Int tile)
	{
		if (!Map.ContainsPos(tile)) return true;
		if (!Map[tile].IsWalkable) return true;

		foreach (BaseUnit unit in _units)
		{
			if (Map.CellPosFromWorldSpace(unit.Pos) == tile) return true;
		}

		return false;
	}

	public static WorldState Load(string file)
	{
		using FileStream fs = File.OpenRead(file);
		using BrotliStream zip = new BrotliStream(fs, CompressionMode.Decompress);
		using SerializerReader reader = new SerializerReader(zip);

		return reader.Read<WorldState>();
	}

	public void Save(string file)
	{
		using FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write);
		using BrotliStream zip = new BrotliStream(fs, CompressionLevel.Optimal);
		using SerializerWriter writer = new SerializerWriter(zip);

		writer.Write(this);
	}

	public void SetPlayerVersion(int playerId)
	{
		PlayerVersion = playerId;
	}
}
}
