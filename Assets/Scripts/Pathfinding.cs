using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
	private Vector2 _start;
	private Vector2 _goal;
	private Grid<Cell> _grid;
	private Vector2Int _startGrid;
	private Vector2Int _goalGrid;

	public Pathfinding(Grid<Cell> grid)
	{
		_grid = grid;
	}

	public bool HasPath(Vector2 start, Vector2 goal)
	{
		return _grid.ContainsPosFromWorldSpace(start) && _grid.ContainsPosFromWorldSpace(goal);
	}

	public Path GetPath(Vector2 start, Vector2 goal)
	{
		if (!HasPath(start, goal)) return new Path();

		_start = start;
		_goal = goal;

		PriorityQueue<PathNode, float> open = new PriorityQueue<PathNode, float>(Comparer<float>.Create((x, y) => y.CompareTo(x)));
		HashSet<PathNode> openSet = new HashSet<PathNode>();
		HashSet<PathNode> closedSet = new HashSet<PathNode>();

		Vector2 current = start;
		while (!GameMath.Vector2Compare(current, goal))
		{
			// expand nodes
			// save & order nodes by f(x) = g(x) + h(x)
			// explore next node
		}
	}

	public List<Vector2> GetNeighbours(Vector2Int gridPos)
	{

	}
}
