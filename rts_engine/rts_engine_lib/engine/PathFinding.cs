namespace RtsEngine
{
using Map;
using System.Collections.Generic;

#nullable enable

public class PathFinding
{
	private Vec2 _start;
	private Vec2 _goal;
	private Grid<Cell> _grid;
	// private Vec2Int _startGrid;
	// private Vec2Int _goalGrid;

	public PathFinding(Grid<Cell> grid)
	{
		_grid = grid;
	}

	public bool HasPath(Vec2 start, Vec2 goal)
	{
		return _grid.ContainsPosFromWorldSpace(start) && _grid.ContainsPosFromWorldSpace(goal);
	}

	public Path GetPath(Vec2 start, Vec2 goal)
	{
		if (!HasPath(start, goal)) return new Path();

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

		return path;
	}
}
}
