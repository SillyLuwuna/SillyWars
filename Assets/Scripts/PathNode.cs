using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

public class PathNode : IEquatable<PathNode>
{
	public float X { get; private set; }
	public float Y { get; private set; }

	public float G { get; private set; }
	public float H { get; private set; }
	public float F { get; private set; }
	public PathNode? parent { get; private set; }

	private PathNode(float x, float y, float g, PathNode? parent, float goalX, float goalY)
	{
		X = x;
		Y = y;
		G = g;
		H = Heuristic(x, y, goalX, goalY);
		F = G + H;
		this.parent = parent;
	}

	private PathNode(float x, float y, PathNode parent, float goalX, float goalY) :
		this(x, y, parent.G + Distance(parent.X, parent.Y, x, y), parent, goalX, goalY) { }

	public PathNode(float x, float y, float goalX, float goalY) :
		this(x, y, 0, null, goalX, goalY) { }

	private static float Heuristic(float x, float y, float goalX, float goalY)
	{
		return Distance(x, y, goalX, goalY);
	}

	public List<PathNode> Children(Grid<Cell> grid, float goalX, float goalY)
	{
		List<PathNode> children = new List<PathNode>(10);

		Vector2Int gridPos = grid.CellPosFromWorldSpace(X, Y);
		Vector2Int goalGridPos = grid.CellPosFromWorldSpace(goalX, goalY);
		Vector2 gridWorldPos = grid.WorldSpaceFromCellPos(gridPos);

		if (gridPos == goalGridPos)
		{
			children.Add(new PathNode(goalX, goalY, this, goalX, goalY));
		}

		if (!GameMath.Vector2Compare(gridWorldPos, new Vector2(X, Y)))
		{
			children.Add(new PathNode(gridWorldPos.x, gridWorldPos.y, this, goalX, goalY));
			return children;
		}

		if (gridPos.x - 1 >= 0)
		{
			if (grid[gridPos.x - 1, gridPos.y].IsWalkable)
			{
				Vector2 childPos = grid.WorldSpaceFromCellPos(new Vector2Int(gridPos.x - 1, gridPos.y));
				children.Add(new PathNode(childPos.x, childPos.y, this, goalX, goalY));
			}

			if (gridPos.y - 1 >= 0 && grid[gridPos.x - 1, gridPos.y - 1].IsWalkable)
			{
				Vector2 childPosInner = grid.WorldSpaceFromCellPos(new Vector2Int(gridPos.x - 1, gridPos.y - 1));
				children.Add(new PathNode(childPosInner.x, childPosInner.y, this, goalX, goalY));
			}

			if (gridPos.y + 1 < grid.Height && grid[gridPos.x - 1, gridPos.y + 1].IsWalkable)
			{
				Vector2 childPosInner = grid.WorldSpaceFromCellPos(new Vector2Int(gridPos.x - 1, gridPos.y + 1));
				children.Add(new PathNode(childPosInner.x, childPosInner.y, this, goalX, goalY));
			}
		}

		if (gridPos.x + 1 < grid.Width)
		{
			if (grid[gridPos.x + 1, gridPos.y].IsWalkable)
			{
				Vector2 childPos = grid.WorldSpaceFromCellPos(new Vector2Int(gridPos.x + 1, gridPos.y));
				children.Add(new PathNode(childPos.x, childPos.y, this, goalX, goalY));
			}

			if (gridPos.y - 1 >= 0 && grid[gridPos.x + 1, gridPos.y - 1].IsWalkable)
			{
				Vector2 childPosInner = grid.WorldSpaceFromCellPos(new Vector2Int(gridPos.x + 1, gridPos.y - 1));
				children.Add(new PathNode(childPosInner.x, childPosInner.y, this, goalX, goalY));
			}

			if (gridPos.y + 1 < grid.Height && grid[gridPos.x + 1, gridPos.y + 1].IsWalkable)
			{
				Vector2 childPosInner = grid.WorldSpaceFromCellPos(new Vector2Int(gridPos.x + 1, gridPos.y + 1));
				children.Add(new PathNode(childPosInner.x, childPosInner.y, this, goalX, goalY));
			}
		}

		if (gridPos.y - 1 >= 0 && grid[gridPos.x, gridPos.y - 1].IsWalkable)
		{
			Vector2 childPos = grid.WorldSpaceFromCellPos(new Vector2Int(gridPos.x, gridPos.y - 1));
			children.Add(new PathNode(childPos.x, childPos.y, this, goalX, goalY));
		}

		if (gridPos.y + 1 < grid.Height && grid[gridPos.x, gridPos.y + 1].IsWalkable)
		{
			Vector2 childPos = grid.WorldSpaceFromCellPos(new Vector2Int(gridPos.x, gridPos.y + 1));
			children.Add(new PathNode(childPos.x, childPos.y, this, goalX, goalY));
		}

		return children;
	}

	private static float Distance(float x0, float y0, float x1, float y1)
	{
		float dx = x1 - x0;
		float dy = y1 - y0;
		return MathF.Sqrt(dx * dx + dy * dy);
	}

	private bool TrueEquals(PathNode other)
	{
		return GameMath.FloatCompare(X, other.X) && GameMath.FloatCompare(Y, other.Y);
	}

	public bool Equals(PathNode other)
	{
		// if (other == null) return false;
		return GameMath.FloatCompare(X, other.X) && GameMath.FloatCompare(Y, other.Y);
		// return TrueEquals(other);
	}

	public override bool Equals(object other)
	{
		if (other == null || !(other is PathNode)) // could be simplified?
		{
			return false;
		}
		return Equals((PathNode) other);
		// return TrueEquals((PathNode) other);
	}

	public static bool operator ==(PathNode? left, PathNode? right)
	{
		if (left is null && right is null) return true;
		if (left is null || right is null) return false;

		return left.Equals(right);
		// return left.TrueEquals(right);
	}

	public static bool operator !=(PathNode? left, PathNode? right)
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
