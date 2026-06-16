namespace RtsEngine;
using Units;
using Map;
using System.IO.Compression;

public class WorldState
{
	public SerializableGrid<Cell> Map;
	public List<Entity> Entities;
	public List<BaseUnit> Units;

	public WorldState(SerializableGrid<Cell> map)
	{
		Map = map;
		Entities = new();
		Units = new();
	}

	public static WorldState Load(string file)
	{
		using FileStream fs = File.OpenRead(file);
		using GZipStream gzip = new GZipStream(fs, CompressionMode.Decompress);
		using BinaryReader reader = new BinaryReader(gzip);

		SerializableGrid<Cell> map = Serializer.Deserialize<SerializableGrid<Cell>>(reader);

		List<Entity> entities = new();
		List<BaseUnit> units = new();
		int entitiesNum = reader.ReadInt32();
		for (int i = 0; i < entitiesNum; i++)
		{
			Entity curr = Serializer.Deserialize<Entity>(reader);
			entities.Add(curr);
			if (curr is BaseUnit currBase)
			{
				units.Add(currBase);
			}
		}

		WorldState state = new WorldState(map);
		state.Entities = entities;

		return state;
	}

	public void Save(string file)
	{
		using FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write);
		using GZipStream gzip = new GZipStream(fs, CompressionLevel.Fastest);
		using BinaryWriter writer = new BinaryWriter(gzip);

		Serializer.Serialize(writer, Map);

		writer.Write(Entities.Count);
		foreach (Entity entity in Entities)
		{
			Serializer.Serialize(writer, entity);
		}
	}
}
