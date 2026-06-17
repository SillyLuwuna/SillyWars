using System;
using System.Collections.Generic;
using RtsEngine.Math;

namespace RtsEngine.Map
{

public class Path : IEquatable<Path>
{
	List<Vec2> _path;

	public Path()
	{
		_path = new List<Vec2>();
	}

	public int Count
	{
		get => _path.Count;
	}

	public void Add(Vec2 point)
	{
		_path.Add(point);
	}

	public void Reverse()
	{
		_path.Reverse();
	}

	// public Vec2 PointAt(int i)
	// {
	// 	return _path[i];
	// }

	public Vec2 Last => _path[_path.Count - 1];

	public Vec2 this[int i]
	{
		get => _path[i];
		private set => _path[i] = value;
	}

	public float Length
	{
		get
		{
			int pathSize = _path.Count;
			float len = 0;
			for (int i = 0; i < pathSize - 1; i++)
			{
				len += _path[i].Distance(_path[i + 1]);
			}
			return len;
		}
	}

	public Path Clone()
	{
		Path clone = new Path();
		int pathSize = _path.Count;
		for (int i = 0; i < pathSize; i++)
		{
			Vec2 original = _path[i];
			clone._path.Add(new Vec2(original.x, original.y));
		}
		return clone;
	}

	public override bool Equals(object? other)
	{
		if (other == null || !(other is Path)) // could be simplified?
		{
			return false;
		}
		return Equals((Path) other);
	}

	public bool Equals(Path? other)
	{
		if (other == null) return false;
		if (_path.Count != other._path.Count) return false;

		int pathSize = _path.Count;
		for (int i = 0; i < pathSize; i++)
		{
			if (!F.Eq(_path[i].x, other._path[i].x)) return false;
			if (!F.Eq(_path[i].y, other._path[i].y)) return false;
		}

		return true;
	}

	public static bool operator ==(Path? left, Path? right)
	{
		if (left is null) return right is null;
		return left.Equals(right);
	}

	public static bool operator !=(Path left, Path right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();

		int pathSize = _path.Count;
		for (int i = 0; i < pathSize; i++)
		{
			hash.Add(F.Hash(_path[i].x));
			hash.Add(F.Hash(_path[i].y));
		}

		return hash.ToHashCode();
	}
}
}
