using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RtsEngine.Data
{

public class SerializerReader : BinaryReader
{
	public SerializerReader(Stream stream) : base(stream)
	{
	}

	public T Read<T>()
	{
		return Deserialize<T>();
	}

	public void Read<T>(out T value)
	{
		value = Deserialize<T>();
	}

	public T Deserialize<T>()
	{
		if (SerializerTypeInfo<T>.IsNullable)
		{
			return DeserializeNullable<T>();
		}
		else if (SerializerTypeInfo<T>.IsSerializable)
		{
			return DeserializeClasses<T>();
		}
		else if (SerializerTypeInfo<T>.IsUnmanaged)
		{
			return DeserializeBase<T>();
		}
		else if (SerializerTypeInfo<T>.IsArray)
		{
			return DeserializeArray<T>();
		}
		else if (SerializerTypeInfo<T>.IsList)
		{
			return DeserializeList<T>();
		}
		else
		{
			throw new ArgumentException($"Type {typeof(T)} is not serializable");
		}
	}

	private T DeserializeClasses<T>()
	{
		string typeName = base.ReadString();
		Type type = Type.GetType(typeName) ?? throw new InvalidDataException("Null type deserialization.");

		object instance = RuntimeHelpers.GetUninitializedObject(type);
		((ISerializable)instance).DeserializeFields(this);

		return (T)instance;
	}

	private T DeserializeBase<T>()
	{
		Type type = typeof(T);
		type = type.IsEnum ? Enum.GetUnderlyingType(type) : type;

		int unmanagedSize = SerializerTypeInfo<T>.UnmanagedSize;
		byte[] buffer = this.ReadBytes(unmanagedSize);

		T instance;

		IntPtr ptr = Marshal.AllocHGlobal(unmanagedSize);
		try
		{
			Marshal.Copy(buffer, 0, ptr, unmanagedSize);
			instance = (T)Marshal.PtrToStructure(ptr, type);
		}
		finally
		{
			Marshal.FreeHGlobal(ptr);
		}


		return instance;
	}

	private T DeserializeArray<T>()
	{
		Type elementType = SerializerTypeInfo<T>.ArrayElementType;
		int length = this.Read<int>();

		Array array = Array.CreateInstance(elementType, length);
		Func<object> deserializer = GetDeserializer(elementType);

		for (int i = 0; i < length; i++)
		{
			array.SetValue(deserializer(), i);
		}

		return (T)(object)array;
	}

	// should be cached
	private Func<object> GetDeserializer(Type type)
	{
		MethodInfo? method = typeof(SerializerReader).GetMethod(nameof(Deserialize), Type.EmptyTypes).MakeGenericMethod(type);
		Func<object> deserializer = () => method.Invoke(this, null);
		return deserializer;
	}

	private T DeserializeList<T>()
	{
		Type elementType = SerializerTypeInfo<T>.ListElementType;
		int length = this.Read<int>();

		IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
		Func<object> deserializer = GetDeserializer(elementType);

		for (int i = 0; i < length; i++)
		{
			list.Add(deserializer());
		}

		return (T)list;
	}

	private T DeserializeNullable<T>()
	{
		bool isNull = Read<bool>();
		if (isNull) return (T)(object)null!;

		Func<object> deserializer = GetDeserializer(SerializerTypeInfo<T>.NullableUnderlyingType);
		return (T)deserializer();
	}
}
}
