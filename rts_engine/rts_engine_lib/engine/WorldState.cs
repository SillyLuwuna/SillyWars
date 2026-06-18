using RtsEngine.Data;
using RtsEngine.Units;
using RtsEngine.Map;
using System.IO.Compression;

using System.IO;
using System.Collections.Generic;
using RtsEngine.EntityProperties;
using System;

namespace RtsEngine
{

public class WorldState : ISerializable
{
	public SerializableGrid<Cell> Map = null!;
	private Dictionary<uint, Entity> _entitiesId = null!;
	private List<Entity> _entities = null!;
	private List<BaseUnit> _units = null!;

	public WorldState(SerializableGrid<Cell> map)
	{
		// TODO fix warning on old compiler version
		Init(map);
	}

	// [MemberNotNull(nameof(Map), nameof(Entities), nameof(Units))]
	private void Init(SerializableGrid<Cell> map)
	{
		Map = map;
		_entitiesId = new Dictionary<uint, Entity>();
		_entities = new List<Entity>();
		_units = new List<BaseUnit>();
	}

	public void SerializeFields(BinaryWriter writer)
	{
		Serializer.Serialize(writer, Map);

		writer.Write(_entities.Count);
		foreach (Entity entity in _entities)
		{
			Serializer.Serialize(writer, entity);
		}
	}

	public void DeserializeFields(BinaryReader reader)
	{
		SerializableGrid<Cell> map = Serializer.Deserialize<SerializableGrid<Cell>>(reader);
		Init(map);

		int entitiesNum = reader.ReadInt32();
		for (int i = 0; i < entitiesNum; i++)
		{
			Entity curr = Serializer.Deserialize<Entity>(reader);
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

	public static WorldState Load(string file)
	{
		using FileStream fs = File.OpenRead(file);
		using GZipStream gzip = new GZipStream(fs, CompressionMode.Decompress);
		using BinaryReader reader = new BinaryReader(gzip);

		return Serializer.Deserialize<WorldState>(reader);
	}

	public void Save(string file)
	{
		using FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write);
		using GZipStream gzip = new GZipStream(fs, CompressionLevel.Fastest);
		using BinaryWriter writer = new BinaryWriter(gzip);

		Serializer.Serialize(writer, this);
	}
}
}
