namespace RtsEngine;

public class Cell : ISerializable<Cell>
{
	public bool IsWalkable;

	public Cell() { }

	public Cell(bool isWalkable)
	{
		IsWalkable = isWalkable;
	}

	public void Serialize(BinaryWriter writer)
	{
		writer.Write(IsWalkable);
	}

	public void Deserialize(BinaryReader reader)
	{
		IsWalkable = reader.ReadBoolean();
	}
}
