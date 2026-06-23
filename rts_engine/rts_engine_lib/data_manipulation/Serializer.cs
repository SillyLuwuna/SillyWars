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
}
}
