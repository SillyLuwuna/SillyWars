using System;

public struct PathNode : IEquatable<PathNode>
{
	public float X;
	public float Y;

	public float G;
	public float H;
	public float F => G + H;

	public bool Equals (PathNode other)
	{
		return GameMath.FloatCompare(X, other.X) && GameMath.FloatCompare(Y, other.Y);
	}

	public override bool Equals(object other)
	{
		if (other == null || !(other is PathNode)) // could be simplified?
		{
			return false;
		}
		return Equals((PathNode) other);
	}

	public static bool operator ==(PathNode left, PathNode right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PathNode left, PathNode right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();

		hash.Add(GameMath.RoundToEpsilonHash(X));
		hash.Add(GameMath.RoundToEpsilonHash(Y));

		return hash.ToHashCode();
	}
}
