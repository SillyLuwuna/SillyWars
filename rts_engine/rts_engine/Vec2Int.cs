namespace RtsEngine;

public struct Vec2Int : IEquatable<Vec2Int>, ISerializable<Vec2Int>
{
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
		float dy = other.y = y;
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

	public override bool Equals(object? obj)
	{
		if (obj is not Vec2Int other) return false;
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

	public void Serialize(BinaryWriter writer)
	{
		writer.Write(x);
		writer.Write(y);
	}

	public void Deserialize(BinaryReader reader)
	{
		x = reader.ReadInt32();
		y = reader.ReadInt32();
	}
}
