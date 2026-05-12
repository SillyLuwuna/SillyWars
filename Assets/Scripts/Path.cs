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

	}

	public static bool operator ==(Path left, Path right)
	{

	}

	public static bool operator !=(Path left, Path right)
	{

	}

	public bool Equals(Path other)
	{

	}

	public override int GetHashCode()
	{

	}
}
