namespace RtsEngine;

// public interface ISerializable<T>
public interface ISerializable
{
	public void SerializeFields(BinaryWriter writer);
	public void DeserializeFields(BinaryReader reader);
}
