public interface ISerializable<T> where T : new()
{
	void Serialize(BinaryWriter writer);
	void Deserialize(BinaryReader reader);

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
