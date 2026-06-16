namespace RtsEngine;

public class SerializableGrid<T> : Grid<T>, ISerializable<Grid<T>> where T : ISerializable<T>, new()
{
	public SerializableGrid() : base() {}
	public SerializableGrid(Vec2 start, float strideWidth, uint width, uint height) : base(start, strideWidth, width, height) { }

	public void Serialize(BinaryWriter writer)
	{
		_start.Serialize(writer);
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

			((ISerializable<T>)curr).Serialize(writer);
		}
	}

	public void Deserialize(BinaryReader reader)
	{
		Vec2 start = new Vec2();
		start.Deserialize(reader);
		float strideWidth = reader.ReadSingle();
		uint width = reader.ReadUInt32();
		uint height = reader.ReadUInt32();

		Construct(start, strideWidth, width, height);

		uint gridSize = width * height;

		for (int i = 0; i < gridSize; i++)
		{
			bool hasValue = reader.ReadBoolean();
			if (!hasValue) continue;

			T curr = new T();
			((ISerializable<T>)curr).Deserialize(reader);
			_grid[i] = curr;
		}
	}
}

