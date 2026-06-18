using System;
using System.IO;
using RtsEngine.Data;

namespace RtsEngine.Math
{

public struct Vec2 : IEquatable<Vec2>, ISerializable
{
	public float x;
	public float y;

	public Vec2(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public float Distance(Vec2 other)
	{
		float dx = other.x - x;
		float dy = other.y - y;
		return MathF.Sqrt(dx * dx + dy * dy);
	}

	public void Swap()
	{
		float tmp = x;
		x = y;
		y = tmp;
	}

	public static bool operator ==(Vec2 left, Vec2 right)
	{
		return F.Eq(left.x, right.x) && F.Eq(left.y, right.y);
	}

	public static bool operator !=(Vec2 left, Vec2 right)
	{
		return !(left == right);
	}

	public static Vec2 operator -(Vec2 left, Vec2 right)
	{
		return new Vec2(left.x - right.x, left.y - right.y);
	}

	public static Vec2 operator +(Vec2 left, Vec2 right)
	{
		return new Vec2(left.x + right.x, left.y + right.y);
	}

	public static Vec2 operator /(Vec2 left, float right)
	{
		return new Vec2(left.x / right, left.y / right);
	}

	public static Vec2 operator *(Vec2 left, float right)
	{
		return new Vec2(left.x * right, left.y * right);
	}

	public override bool Equals(object? obj)
	{
		if (!(obj is Vec2 other)) return false;
		return this == (Vec2)obj;
	}

	public bool Equals(Vec2 other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(F.Hash(x), F.Hash(y));
	}

	public override string ToString()
	{
		return "(" + x + ", " + y + ")";
	}

	public void SerializeFields(BinaryWriter writer)
	{
		writer.Write(x);
		writer.Write(y);
	}

	public void DeserializeFields(BinaryReader reader)
	{
		x = reader.ReadSingle();
		y = reader.ReadSingle();
	}

	public Vec2 To(Vec2 other)
	{
		return other - this;
	}

	public float Magnitude
	{
		get => System.MathF.Sqrt(x * x + y * y);
	}

	public Vec2 Unit
	{
		get => this / Magnitude;
	}
}
}
