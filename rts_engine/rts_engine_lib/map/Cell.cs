using System.IO;
using RtsEngine.Data;

namespace RtsEngine.Map
{

public class Cell : ISerializable
{
	public CellType Type { get; private set; }
	public bool IsWalkable
	{
		get => !(Type == CellType.Empty || Type == CellType.Structure);
	}

	public Cell() { }

	// public Cell(bool isWalkable)
	public Cell(CellType type)
	{
		Type = type;
	}

	public void SerializeFields(SerializerWriter writer)
	{
		// writer.Write((byte)Type);
		writer.Write(Type);
	}

	public void DeserializeFields(SerializerReader reader)
	{
		// Type = (CellType)reader.ReadByte();
		Type = reader.Read<CellType>();
	}
}
}
