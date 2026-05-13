using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

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

		// PriorityQueue<PathNode, float> open = new PriorityQueue<PathNode, float>(Comparer<float>.Create((x, y) => y.CompareTo(x)));
		PriorityQueue<PathNode, float> open = new PriorityQueue<PathNode, float>();
		Dictionary<PathNode, PathNode> openSet = new Dictionary<PathNode, PathNode>();
		Dictionary<PathNode, PathNode> closedSet = new Dictionary<PathNode, PathNode>();

		PathNode startNode = new PathNode(start.x, start.y, goal.x, goal.y);
		PathNode goalNode = new PathNode(goal.x, goal.y, goal.x, goal.y);
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

			if (!GameMath.FloatLte(curr.G, openSet[curr].G))
			{
				// a better version of this node was found and awaits processing
				continue;
			}

			openSet.Remove(curr);
			closedSet.Add(curr, curr);

			// expand nodes
			List<PathNode> neighbours = curr.Children(_grid, goal.x, goal.y);

			// save & order nodes by f(x) = g(x) + h(x)
			for (int i = 0; i < neighbours.Count; i++)
			{
				PathNode currNeighbour = neighbours[i];

				if (openSet.ContainsKey(currNeighbour)) // can be optimized to check hashmap once
				{
					if (GameMath.FloatLte(openSet[currNeighbour].G, currNeighbour.G))
					{
						continue;
					}

					openSet.Remove(currNeighbour);
				}

				if (closedSet.ContainsKey(currNeighbour)) // can be optimized to check hashmap once
				{
					if (GameMath.FloatLte(closedSet[currNeighbour].G, currNeighbour.G))
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
			path.Add(new Vector2(curr.X, curr.Y));
			curr = curr.parent;
		}
		path.Reverse();

		return path;
	}
}
