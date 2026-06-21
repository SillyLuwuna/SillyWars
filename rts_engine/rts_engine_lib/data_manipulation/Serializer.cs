using System.IO;

namespace RtsEngine.Data
{

public static class Serializer
{
	public static byte[] ToBytes<T>(T obj)
	{
		using MemoryStream ms = new MemoryStream();
		using SerializerWriter writer = new SerializerWriter(ms);

		// writer.Serialize(obj);
		writer.Write(obj);

		return ms.ToArray();
	}

	public static T FromBytes<T>(byte[] data)
	{
		using MemoryStream ms = new MemoryStream(data);
		using SerializerReader reader = new SerializerReader(ms);

		// return reader.Deserialize<T>();
		return reader.Read<T>();
	}

	// public static T FromBytes<T>(byte[] data, out byte[] remainder) where T : ISerializable
	// {
	// 	using MemoryStream ms = new MemoryStream(data);
	// 	using SerializerReader reader = new SerializerReader(ms);
	//
	// 	T obj = Deserialize<T>(reader);
	// 	long remainderLen = reader.BaseStream.Length - reader.BaseStream.Position;
	// 	remainder = reader.ReadBytes((int)remainderLen);
	// 	return obj;
	// }
}
}
