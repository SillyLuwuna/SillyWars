using RtsEngine.Data;
using RtsEngine.Units;
using RtsEngine.Map;
using System.IO.Compression;

using System.IO;
using System.Collections.Generic;
using RtsEngine.EntityProperties;
using System;
using RtsEngine.Physics;

namespace RtsEngine
{

public class WorldState : ISerializable
{
	public Grid<Cell> Map = null!;
	private Dictionary<uint, Entity> _entitiesId = null!;
	private List<Entity> _entities = null!;
	private List<BaseUnit> _units = null!;
	private List<PhysicsObject> _physicsObjects = null!;

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
	}

	public void SerializeFields(SerializerWriter writer)
	{
		writer.Write(Map);

		writer.Write(_entities.Count);
		foreach (Entity entity in _entities)
		{
			writer.Write(entity);
		}
	}

	public void DeserializeFields(SerializerReader reader)
	{
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
		_entities.Add(entity);
		_entitiesId.Add(entity.Id, entity);

		if (entity is BaseUnit currBase)
		{
			_units.Add(currBase);
		}

		if (entity is PhysicsObject currPhysics)
		{
			_physicsObjects.Add(currPhysics);
		}
	}

	public void TickEntities()
	{
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
			}
		}
	}

	public Entity? GetEntity(uint id)
	{
		if (!_entitiesId.ContainsKey(id)) return null;

		return _entitiesId[id];
	}

	public void RemoveEntity(Entity entity)
	{
		// can be optimized by lazy deleting from arrays
		_entities.Remove(entity);
		_entitiesId.Remove(entity.Id);
		if (entity is BaseUnit currBase)
		{
			_units.Remove(currBase);
		}
	}

	public List<BaseUnit> GetUnitView() // TODO make actual view
	{
		return _units;
	}

	public List<PhysicsObject> GetPhysicsObjects()
	{
		return _physicsObjects;
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
}
}
