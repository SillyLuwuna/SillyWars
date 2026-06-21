// using System.Runtime.Serialization
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RtsEngine.Data
{

public static class SerializerTypeInfo<T>
{
	public static readonly Type TType = typeof(T);

	public static readonly bool IsUnmanaged = GetIsUnmanaged(TType);
	public static readonly bool IsSerializable = GetIsSerializable(TType);
	public static readonly int UnmanagedSize = IsUnmanaged ? GetUnmanagedSize(TType) : 0;

	public static readonly bool IsArray = TType.IsArray;
	public static readonly Type ArrayElementType = IsArray ? TType.GetElementType()! : null!;

	public static readonly bool IsList = GetIsList(TType);
	public static readonly Type ListElementType = IsList ? TType.GetGenericArguments()[0] : null!;


	private static bool GetIsSerializable(Type type)
	{
		return typeof(ISerializable).IsAssignableFrom(type);
	}

	private static bool GetIsUnmanaged(Type type)
	{
		return (type.IsValueType && GetAreAllFieldsUnmanaged(type));
	}

	private static bool GetAreAllFieldsUnmanaged(Type type)
	{
		if (!type.IsValueType) return false;
		if (type.IsPrimitive || type == typeof(decimal) || type.IsEnum) return true;

		return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			.All(f => GetAreAllFieldsUnmanaged(f.FieldType));
	}

	private static bool GetIsList(Type type)
	{
		return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
	}

	private static int GetUnmanagedSize(Type type)
	{
		type = type.IsEnum ? type.GetEnumUnderlyingType() : type;
		return Marshal.SizeOf(type);
	}
}
}
