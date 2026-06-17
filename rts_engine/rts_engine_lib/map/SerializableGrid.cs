using System.IO;
using RtsEngine.Data;
using RtsEngine.Math;

namespace RtsEngine.Map
{

// public class SerializableGrid<T> : Grid<T>, ISerializable where T : ISerializable
public class SerializableGrid<T> : Grid<T>, ISerializable where T : class, ISerializable
{
	public SerializableGrid() : base() {}
	public SerializableGrid(Vec2 start, float strideWidth, uint width, uint height)
	{
		base.Construct(start, strideWidth, width, height);
	}

	public void SerializeFields(BinaryWriter writer)
	{
		Serializer.Serialize(writer, _start);
		writer.Write(_strideWidth);
		writer.Write(_width);
		writer.Write(_height);

		uint gridSize = _width * _height;

		for (int i = 0; i < gridSize; i++)
		{
			T? curr = _grid[i];

			bool hasValue = curr != null;
			writer.Write(hasValue);

			if (!hasValue || curr == null) // same thing but appeases compiler
			{
				continue;
			}

			Serializer.Serialize(writer, curr);
		}
	}

	public void DeserializeFields(BinaryReader reader)
	{
		Vec2 start = Serializer.Deserialize<Vec2>(reader);
		float strideWidth = reader.ReadSingle();
		uint width = reader.ReadUInt32();
		uint height = reader.ReadUInt32();

		uint gridSize = width * height;

		base.Construct(start, strideWidth, width, height);

		for (int i = 0; i < gridSize; i++)
		{
			bool hasValue = reader.ReadBoolean();
			if (!hasValue) continue;

			_grid[i] = Serializer.Deserialize<T>(reader);
		}
	}
}

}
