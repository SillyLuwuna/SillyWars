using RtsEngine.Data;
using RtsEngine.Units;
using RtsEngine.Map;
using System.IO.Compression;

using System.IO;
using System.Collections.Generic;
using RtsEngine.EntityProperties;

namespace RtsEngine
{

public class WorldState : ISerializable
{
	public SerializableGrid<Cell> Map = null!;
	public List<Entity> Entities = null!;
	public List<BaseUnit> Units = null!;

	public WorldState(SerializableGrid<Cell> map)
	{
		// TODO fix warning on old compiler version
		Init(map);
	}

	// [MemberNotNull(nameof(Map), nameof(Entities), nameof(Units))]
	private void Init(SerializableGrid<Cell> map)
	{
		Map = map;
		Entities = new List<Entity>();
		Units = new List<BaseUnit>();
	}

	public void SerializeFields(BinaryWriter writer)
	{
		Serializer.Serialize(writer, Map);

		writer.Write(Entities.Count);
		foreach (Entity entity in Entities)
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
			Entities.Add(curr);
			if (curr is BaseUnit currBase)
			{
				Units.Add(currBase);
			}
		}
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
