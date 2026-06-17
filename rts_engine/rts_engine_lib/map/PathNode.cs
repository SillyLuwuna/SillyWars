using RtsEngine.Map;
using RtsEngine.Math;
using System;
using System.Collections.Generic;

namespace RtsEngine.Map
{

#nullable enable

public class PathNode : IEquatable<PathNode>
{
	public Vec2 Pos { get; private set; }

	public float G { get; private set; }
	public float H { get; private set; }
	public float F { get; private set; }
	public PathNode? parent { get; private set; }

	private PathNode(Vec2 pos, float g, PathNode? parent, Vec2 goal)
	{
		Pos = pos;
		G = g;
		H = Heuristic(pos, goal);
		F = G + H;
		this.parent = parent;
	}

	private PathNode(Vec2 pos, PathNode parent, Vec2 goal) :
		this(pos, parent.G + pos.Distance(goal), parent, goal) { }

	public PathNode(Vec2 pos, Vec2 goal) :
		this(pos, 0, null, goal) { }

	private static float Heuristic(Vec2 pos, Vec2 goal)
	{
		return pos.Distance(goal);
	}

	public List<PathNode> Children(Grid<Cell> grid, Vec2 goal)
	{
		List<PathNode> children = new List<PathNode>(10);

		Vec2Int gridPos = grid.CellPosFromWorldSpace(goal);
		Vec2Int goalGridPos = grid.CellPosFromWorldSpace(goal);
		Vec2 gridWorldPos = grid.WorldSpaceFromCellPos(gridPos);

		if (gridPos == goalGridPos)
		{
			children.Add(new PathNode(goal, this, goal));
		}

		if (gridWorldPos != Pos)
		{
			children.Add(new PathNode(gridWorldPos, this, goal));
			return children;
		}

		if (gridPos.x - 1 >= 0)
		{
			if (grid[gridPos.x - 1, gridPos.y].IsWalkable)
			{
				Vec2 childPos = grid.WorldSpaceFromCellPos(new Vec2Int(gridPos.x - 1, gridPos.y));
				children.Add(new PathNode(childPos, this, goal));
			}

			if (gridPos.y - 1 >= 0 && grid[gridPos.x - 1, gridPos.y - 1].IsWalkable)
			{
				Vec2 childPosInner = grid.WorldSpaceFromCellPos(new Vec2Int(gridPos.x - 1, gridPos.y - 1));
				children.Add(new PathNode(childPosInner, this, goal));
			}

			if (gridPos.y + 1 < grid.Height && grid[gridPos.x - 1, gridPos.y + 1].IsWalkable)
			{
				Vec2 childPosInner = grid.WorldSpaceFromCellPos(new Vec2Int(gridPos.x - 1, gridPos.y + 1));
				children.Add(new PathNode(childPosInner, this, goal));
			}
		}

		if (gridPos.x + 1 < grid.Width)
		{
			if (grid[gridPos.x + 1, gridPos.y].IsWalkable)
			{
				Vec2 childPos = grid.WorldSpaceFromCellPos(new Vec2Int(gridPos.x + 1, gridPos.y));
				children.Add(new PathNode(childPos, this, goal));
			}

			if (gridPos.y - 1 >= 0 && grid[gridPos.x + 1, gridPos.y - 1].IsWalkable)
			{
				Vec2 childPosInner = grid.WorldSpaceFromCellPos(new Vec2Int(gridPos.x + 1, gridPos.y - 1));
				children.Add(new PathNode(childPosInner, this, goal));
			}

			if (gridPos.y + 1 < grid.Height && grid[gridPos.x + 1, gridPos.y + 1].IsWalkable)
			{
				Vec2 childPosInner = grid.WorldSpaceFromCellPos(new Vec2Int(gridPos.x + 1, gridPos.y + 1));
				children.Add(new PathNode(childPosInner, this, goal));
			}
		}

		if (gridPos.y - 1 >= 0 && grid[gridPos.x, gridPos.y - 1].IsWalkable)
		{
			Vec2 childPos = grid.WorldSpaceFromCellPos(new Vec2Int(gridPos.x, gridPos.y - 1));
			children.Add(new PathNode(childPos, this, goal));
		}

		if (gridPos.y + 1 < grid.Height && grid[gridPos.x, gridPos.y + 1].IsWalkable)
		{
			Vec2 childPos = grid.WorldSpaceFromCellPos(new Vec2Int(gridPos.x, gridPos.y + 1));
			children.Add(new PathNode(childPos, this, goal));
		}

		return children;
	}

	private static float Distance(float x0, float y0, float x1, float y1)
	{
		float dx = x1 - x0;
		float dy = y1 - y0;
		return MathF.Sqrt(dx * dx + dy * dy);
	}

	public bool Equals(PathNode? other)
	{
		if (other == null) return false;
		return this == (PathNode)other;
	}

	public override bool Equals(object? obj)
	{
		if (!(obj is PathNode other)) return false;
		return this == (PathNode)obj;
	}

	public static bool operator ==(PathNode? left, PathNode? right)
	{
		if (left is null && right is null) return true;
		if (left is null || right is null) return false;

		return left.Pos == right.Pos;
	}

	public static bool operator !=(PathNode? left, PathNode? right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return Pos.GetHashCode();
	}
}
}
