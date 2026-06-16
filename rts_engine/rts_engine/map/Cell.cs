namespace RtsEngine.Map;

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

	public void SerializeFields(BinaryWriter writer)
	{
		writer.Write((byte)Type);
	}

	public void DeserializeFields(BinaryReader reader)
	{
		Type = (CellType)reader.ReadByte();
	}
}
