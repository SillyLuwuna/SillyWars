namespace RtsEngine;

// public interface ISerializable<T>
public interface ISerializable
{
	public void SerializeFields(BinaryWriter writer);
	public void DeserializeFields(BinaryReader reader);
	// public abstract static T Deserialize(BinaryReader reader);

	// public 

	// public byte[] ToBytes()
	// {
	// 	using MemoryStream ms = new MemoryStream();
	// 	using BinaryWriter writer = new BinaryWriter(ms);
	//
	// 	Serialize(writer);
	//
	// 	return ms.ToArray();
	// }
	//
	// public void FromBytes(byte[] data)
	// {
	// 	using MemoryStream ms = new MemoryStream(data);
	// 	using BinaryReader reader = new BinaryReader(ms);
	//
	// 	Deserialize(reader);
	// }
}
