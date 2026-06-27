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
using RtsEngine.Resources;

namespace RtsEngine
{

public class WorldState : ISerializable
{
	private static int ResourceNum = Enum.GetValues(typeof(Resource)).Length;

	private const bool DEBUG = true;
	public int NumPlayers { get; private set; }

	public Grid<Cell> Map = null!;
	private Dictionary<uint, Entity> _entitiesId = null!;
	private List<Entity> _entities = null!;
	private List<BaseUnit> _units = null!;
	private List<PhysicsObject> _physicsObjects = null!;
	private List<IDestroyable> _destroyables = null!;
	private List<BaseStructure> _structures = null!;

	private Queue<Entity> _addQueue = null!;
	private Queue<Entity> _removeQueue = null!;
	private bool _isTickingEntities;

	private PathFinder _pathFinder = null!;

	// public List<List<int>> PlayerResources { get; private set; } = null!;
	public int[] _playerResources = null!; // should be private

	public int PlayerVersion { get; private set; }
	public List<uint> AddedEntities { get; private set; } = null!;
	public List<uint> RemovedEntities { get; private set; } = null!;

	public WorldState(Grid<Cell> map, int numPlayers)
	{
		Init(map, numPlayers);
	}

	private void Init(Grid<Cell> map, int numPlayers)
	{
		Map = map;
		NumPlayers = numPlayers;

		_playerResources = new int[numPlayers * ResourceNum];
		// PlayerResources = new List<List<int>>();
		// for (int i = 0; i < numPlayers; i++)
		// {
		// 	PlayerResources.Add(new List<int>());
		// 	for (int j = 0; j < ResourceNum; j++)
		// 	{
		// 		PlayerResources[i].Add(0);
		// 	}
		// }

		_entitiesId = new Dictionary<uint, Entity>();
		_entities = new List<Entity>();
		_units = new List<BaseUnit>();
		_physicsObjects = new List<PhysicsObject>();
		_destroyables = new List<IDestroyable>();
		_structures = new List<BaseStructure>();

		_addQueue = new Queue<Entity>();
		_removeQueue = new Queue<Entity>();
		_isTickingEntities = false;

		_pathFinder = new PathFinder(Map);

		AddedEntities = new List<uint>();
		RemovedEntities = new List<uint>();
	}

	public void SerializeFields(SerializerWriter writer)
	{
		writer.Write(PlayerVersion);

		writer.Write(Map);
		writer.Write(NumPlayers);

		writer.Write(_playerResources);

		writer.Write(_entities.Count);
		foreach (Entity entity in _entities)
		{
			writer.Write(entity);
		}

		writer.Write(AddedEntities);
		writer.Write(RemovedEntities);
	}

	public void DeserializeFields(SerializerReader reader)
	{
		PlayerVersion = reader.Read<int>();

		Grid<Cell> map = reader.Read<Grid<Cell>>();
		int numPlayers = reader.Read<int>();
		Init(map, numPlayers);

		// PlayerResources = reader.Read<List<List<int>>>();
		_playerResources = reader.Read<int[]>();

		int entitiesNum = reader.Read<int>();
		for (int i = 0; i < entitiesNum; i++)
		{
			Entity curr = reader.Read<Entity>();
			AddEntity(curr);
		}

		AddedEntities = reader.Read<List<uint>>();
		RemovedEntities = reader.Read<List<uint>>();
	}

	public void AddEntity(Entity entity)
	{
		if (_isTickingEntities)
		{
			_addQueue.Enqueue(entity);
			return;
		}

		AddedEntities.Add(entity.Id);
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

	public int GetResource(uint playerId, Resource resource)
	{
		return _playerResources[playerId * ResourceNum + (int)resource];
	}

	public void GiveResource(ResourceStack resourceStack, uint playerId)
	{
		_playerResources[playerId * ResourceNum + (int)resourceStack.Resource] += resourceStack.Amount;
		Console.WriteLine($"{playerId}: (+{resourceStack.Amount}) {GetResource(playerId, resourceStack.Resource)}");
	}

	public bool TryTakeResource(ResourceStack resourceStack, uint playerId)
	{
		if (!HasEnoughResources(resourceStack, playerId))
		{
			Console.WriteLine($"{playerId}: not enough {resourceStack.Resource} ({resourceStack.Amount})");
			return false;
		}

		_playerResources[playerId * ResourceNum + (int)resourceStack.Resource] -= resourceStack.Amount;
		Console.WriteLine($"{playerId}: (-{resourceStack.Amount}) {GetResource(playerId, resourceStack.Resource)}");
		return true;
	}

	public bool HasEnoughResources(ResourceStack resourceStack, uint playerId)
	{
		return (_playerResources[playerId * ResourceNum + (int)resourceStack.Resource]) >= resourceStack.Amount;
	}

	public void Tick()
	{
		AddedEntities.Clear();
		RemovedEntities.Clear();

		TickEntities();
		AddQueuedEntities();
		RemoveQueuedEntities();
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

	private void RemoveQueuedEntities()
	{
		while (_removeQueue.Count > 0)
		{
			RemoveEntity(_removeQueue.Dequeue());
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
		if (_isTickingEntities)
		{
			_removeQueue.Enqueue(entity);
			return;
		}

		RemovedEntities.Add(entity.Id);
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

	public List<IDestroyable> Destroyables { get => _destroyables; }

	public List<Entity> Entities { get => _entities; }

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
