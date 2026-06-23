#nullable enable

using RtsEngine.Math;
using System.Collections.Generic;

namespace RtsEngine.Map
{

public class PathFinder
{
	private Vec2 _start;
	private Vec2 _goal;
	private Grid<Cell> _grid;
	private PathOptimizer _optimizer;
	private bool _pathOptimization;

	public PathFinder(Grid<Cell> grid, bool pathOptimization = true)
	{
		_grid = grid;
		_optimizer = new PathOptimizer(grid);
		_pathOptimization = pathOptimization;
	}

	public bool IsPathInGrid(Vec2 start, Vec2 goal)
	{
		return _grid.ContainsPosFromWorldSpace(start) && _grid.ContainsPosFromWorldSpace(goal);
	}

	public bool HasPath(Vec2 start, Vec2 goal)
	{
		Path path = GetPath(start, goal);
		return (path.Count > 0);
	}

	public Path GetPath(Vec2 start, Vec2 goal)
	{
		// should cache stuff
		if (!IsPathInGrid(start, goal)) return new Path();
		if (!IsGoalAttainable(goal)) return new Path();

		_start = start;
		_goal = goal;

		// PriorityQueue<PathNode, float> open = new PriorityQueue<PathNode, float>(Comparer<float>.Create((x, y) => y.CompareTo(x)));
		PriorityQueue<PathNode, float> open = new PriorityQueue<PathNode, float>();
		Dictionary<PathNode, PathNode> openSet = new Dictionary<PathNode, PathNode>();
		Dictionary<PathNode, PathNode> closedSet = new Dictionary<PathNode, PathNode>();

		PathNode startNode = new PathNode(start, goal);
		PathNode goalNode = new PathNode(goal, goal);
		openSet.Add(startNode, startNode);
		open.Enqueue(startNode, startNode.F);
		bool found = false;
		PathNode? curr = null;
		while (open.Count > 0)
		{
			// explore next best node
			curr = open.Dequeue();

			if (curr == goalNode)
			{
				found = true;
				break;
			}

			if (!openSet.ContainsKey(curr))
			{
				// a better version of this node was found and processed first
				continue;
			}

			if (!F.Lte(curr.G, openSet[curr].G))
			{
				// a better version of this node was found and awaits processing
				continue;
			}

			openSet.Remove(curr);
			closedSet.Add(curr, curr);

			// expand nodes
			List<PathNode> neighbours = curr.Children(_grid, goal);

			// save & order nodes by f(x) = g(x) + h(x)
			for (int i = 0; i < neighbours.Count; i++)
			{
				PathNode currNeighbour = neighbours[i];

				if (openSet.ContainsKey(currNeighbour)) // can be optimized to check hashmap once
				{
					if (F.Lte(openSet[currNeighbour].G, currNeighbour.G))
					{
						continue;
					}

					openSet.Remove(currNeighbour);
				}

				if (closedSet.ContainsKey(currNeighbour)) // can be optimized to check hashmap once
				{
					if (F.Lte(closedSet[currNeighbour].G, currNeighbour.G))
					{
						continue;
					}

					closedSet.Remove(currNeighbour);
				}

				open.Enqueue(currNeighbour, currNeighbour.F);
				openSet.Add(currNeighbour, currNeighbour);
			}
		}

		if (!found) return new Path();

		Path path = new Path();
		while (curr != null)
		{
			path.Add(curr.Pos);
			curr = curr.parent;
		}
		path.Reverse();

		if (_pathOptimization)
		{
			path = _optimizer.OptimizePath(path);
		}

		return path;
	}

	private bool IsGoalAttainable(Vec2 goal)
	{
		return _grid.GetObjectAtWorldPos(goal).IsWalkable;
	}
}
}
