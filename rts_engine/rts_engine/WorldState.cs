namespace RtsEngine;

public class WorldState
{
	private SerializableGrid<Cell> _map;

	public WorldState(SerializableGrid<Cell> map)
	{
		_map = map;
	}

	public static WorldState Load(string file)
	{
		using FileStream fs = File.OpenRead(file);
		using BinaryReader reader = new BinaryReader(fs);

		SerializableGrid<Cell> map = new SerializableGrid<Cell>();
		map.Deserialize(reader);

		WorldState state = new WorldState(map);

		return state;
	}

	public void Save(string file)
	{
		using FileStream fs = File.OpenWrite(file);
		using BinaryWriter writer = new BinaryWriter(fs);

		_map.Serialize(writer);
	}

	public SerializableGrid<Cell> GetMapView()
	{
		// TODO make into an actual map view
		return _map;
	}
}
