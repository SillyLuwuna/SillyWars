using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RtsEngine.Data
{

public class SerializerWriter : BinaryWriter
{
	public SerializerWriter(Stream stream) : base(stream)
	{
	}

	public void Write<T>(T obj)
	{
		Serialize(obj);
	}

	public void Serialize<T>(T obj)
	{
		if (SerializerTypeInfo<T>.IsSerializable)
		{
			SerializeClasses<T>(obj);
		}
		else if (SerializerTypeInfo<T>.IsUnmanaged)
		{
			SerializeBase<T>(obj);
		}
		else if (SerializerTypeInfo<T>.IsArray)
		{
			SerializeArray<T>(obj);
		}
		else if (SerializerTypeInfo<T>.IsList)
		{
			SerializeList<T>(obj);
		}
		else
		{
			throw new ArgumentException($"Type {typeof(T)} is not serializable");
		}
	}

	private void SerializeClasses<T>(T obj)
	{
		ISerializable serializable = (obj as ISerializable)!;
		base.Write(serializable.GetType().AssemblyQualifiedName ?? throw new InvalidDataException("Null type serialization."));

		serializable.SerializeFields(this);
	}

	private void SerializeBase<T>(T obj)
	{
		int unmanagedSize = SerializerTypeInfo<T>.UnmanagedSize;
		byte[] buffer = new byte[unmanagedSize];

		IntPtr ptr = Marshal.AllocHGlobal(unmanagedSize);

		try
		{
			object val = typeof(T).IsEnum ? Convert.ChangeType(obj!, Enum.GetUnderlyingType(typeof(T))) : obj!;
			Marshal.StructureToPtr(val, ptr, false);
			Marshal.Copy(ptr, buffer, 0, unmanagedSize);
		}
		finally
		{
			Marshal.FreeHGlobal(ptr);
		}

		base.Write(buffer);
	}

	// should be cached
	private Action<object> GetSerializer(Type type)
	{
		MethodInfo method = typeof(SerializerWriter)
			.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.First(m => m.Name == nameof(Serialize) && m.IsGenericMethod)
			.MakeGenericMethod(type);
		Action<object> serializer = (obj) => method.Invoke(this, new[] { obj });
		return serializer;
	}

	private void SerializeArray<T>(T obj)
	{
		Array objArray = (obj as Array)!;

		Type elementType = SerializerTypeInfo<T>.ArrayElementType;
		Action<object> serializer = GetSerializer(elementType);

		this.Write(objArray.Length);
		for (int i = 0; i < objArray.Length; i++)
		{
			serializer(objArray.GetValue(i));
		}
	}

	private void SerializeList<T>(T obj)
	{
		IList objList = (obj as IList)!;

		Type elementType = SerializerTypeInfo<T>.ListElementType;
		Action<object> serializer = GetSerializer(elementType);

		this.Write(objList.Count);
		for (int i = 0; i < objList.Count; i++)
		{
			serializer(objList[i]);
		}
	}
}
}
