using System;
using System.Collections.Generic;
using UnityEngine;

public class Path : IEquatable<Path>
{
	List<Vector2> _path;

	public Path()
	{
		_path = new List<Vector2>();
	}

	public int PointCount
	{
		get => _path.Count;
	}

	public void Add(Vector2 point)
	{
		_path.Add(point);
	}

	public void Reverse()
	{
		_path.Reverse();
	}

	public Vector2 PointAt(int i)
	{
		return _path[i];
	}

	public float Length
	{
		get
		{
			int pathSize = _path.Count;
			float len = 0;
			for (int i = 0; i < pathSize - 1; i++)
			{
				len += Vector2.Distance(_path[i], _path[i + 1]);
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
			Vector2 original = _path[i];
			clone._path.Add(new Vector2(original.x, original.y));
		}
		return clone;
	}

	public override bool Equals(object other)
	{
		if (other == null || !(other is Path)) // could be simplified?
		{
			return false;
		}
		return Equals((Path) other);
	}

	public bool Equals(Path other)
	{
		if (other == null) return false;
		if (_path.Count != other._path.Count) return false;

		int pathSize = _path.Count;
		for (int i = 0; i < pathSize; i++)
		{
			if (!GameMath.FloatEq(_path[i].x, other._path[i].x)) return false;
			if (!GameMath.FloatEq(_path[i].y, other._path[i].y)) return false;
		}

		return true;
	}

	public static bool operator ==(Path left, Path right)
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
			hash.Add(GameMath.RoundToEpsilonHash(_path[i].x));
			hash.Add(GameMath.RoundToEpsilonHash(_path[i].y));
		}

		return hash.ToHashCode();
	}
}
