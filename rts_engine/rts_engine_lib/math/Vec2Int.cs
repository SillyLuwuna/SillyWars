using System;
using System.IO;
using RtsEngine.Data;

namespace RtsEngine.Math
{

public struct Vec2Int : IEquatable<Vec2Int>
{
	private static readonly Vec2Int ZERO = new Vec2Int(0, 0);

	public int x;
	public int y;

	public Vec2Int(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public float Distance(Vec2Int other)
	{
		float dx = other.x - x;
		float dy = other.y - y;
		return MathF.Sqrt(dx * dx + dy * dy);
	}

	public static bool operator ==(Vec2Int left, Vec2Int right)
	{
		return left.x == right.x && left.y == right.y;
	}

	public static bool operator !=(Vec2Int left, Vec2Int right)
	{
		return !(left == right);
	}

	public static Vec2Int operator -(Vec2Int left, Vec2Int right)
	{
		return new Vec2Int(left.x - right.x, left.y - right.y);
	}

	public static Vec2Int operator -(Vec2Int right)
	{
		return new Vec2Int(-right.x, -right.y);
	}

	public static Vec2Int operator +(Vec2Int left, Vec2Int right)
	{
		return new Vec2Int(left.x + right.x, left.y + right.y);
	}

	public static Vec2Int operator /(Vec2Int left, int right)
	{
		return new Vec2Int(left.x / right, left.y / right);
	}

	public static Vec2Int operator *(Vec2Int left, int right)
	{
		return new Vec2Int(left.x * right, left.y * right);
	}

	public static Vec2Int operator %(Vec2Int left, Vec2Int right)
	{
		return new Vec2Int(left.x % right.x, left.y % right.y);
	}


	public override bool Equals(object? obj)
	{
		// if (obj is not Vec2Int other) return false;
		if (!(obj is Vec2Int other)) return false;
		return this == (Vec2Int)obj;
	}

	public bool Equals(Vec2Int other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(x, y);
	}

	public override string ToString()
	{
		return "(" + x + ", " + y + ")";
	}

	public bool IsZero
	{
		get => this == ZERO;
	}

	public static Vec2Int Zero
	{
		get => new Vec2Int(0, 0);
	}
}
}
