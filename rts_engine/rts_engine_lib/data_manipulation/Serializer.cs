// using System.Runtime.Serialization
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace RtsEngine.Data
{

public static class Serializer
{
	public static T Deserialize<T>(BinaryReader reader) where T : ISerializable
	{
		string typeName = reader.ReadString();
		Type type = Type.GetType(typeName)
			?? throw new InvalidDataException("Null type deserialization.");

		T instance = (T)(RuntimeHelpers.GetUninitializedObject(type));
		// T instance = (T)(Activator.CreateInstance(type, true) ?? throw new InvalidDataException("Null type deserialization."));

		if (!(instance is ISerializable))
		{
			throw new InvalidDataException("Deserializing non-serializable class.");
		}

		instance.DeserializeFields(reader);

		return instance;
	}

	public static void Serialize<T>(BinaryWriter writer, T obj) where T : ISerializable
	{
		writer.Write(obj.GetType().AssemblyQualifiedName ?? throw new InvalidDataException("Null type serialization."));

		obj.SerializeFields(writer);
	}

	public static byte[] ToBytes<T>(T obj) where T : ISerializable
	{
		using MemoryStream ms = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(ms);

		Serialize(writer, obj);

		return ms.ToArray();
	}

	public static T FromBytes<T>(byte[] data) where T : ISerializable
	{
		using MemoryStream ms = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(ms);

		return Deserialize<T>(reader);
	}
}
}
