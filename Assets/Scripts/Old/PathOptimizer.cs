using System.Collections.Generic;
using UnityEngine;

public class PathOptimizer
{
	private Grid<Cell> _grid;
	private GridRaycast<Cell> _gridRaycast;

	public PathOptimizer(Grid<Cell> grid)
	{
		_grid = grid;
		_gridRaycast = new GridRaycast<Cell>(grid);
	}

	public Path OptimizePath(Path path)
	{
		Path optimizedPath = new Path();

		if (path.PointCount < 3)
		{
			return path;
		}

		int curr = 0;
		int next = 1;
		int trySkip = 2;
		optimizedPath.Add(path.PointAt(curr));

		while (trySkip < path.PointCount)
		{
			List<Vector2Int> collisions = _gridRaycast.CastRay(path.PointAt(curr), path.PointAt(trySkip));

			bool isSkippable = true;
			for (int j = 0; j < collisions.Count; j++)
			{
				Vector2Int collision = collisions[j];
				if (!_grid[collision.x, collision.y].IsWalkable)
				{
					isSkippable = false;
					break;
				}
			}
			
			if (isSkippable)
			{
				next = trySkip;
				trySkip++;
			}
			else
			{
				curr = next;
				next = curr + 1;
				trySkip = curr + 2;
				optimizedPath.Add(path.PointAt(curr));
			}
		}

		optimizedPath.Add(path.PointAt(path.PointCount - 1));

		return optimizedPath;
	}
}
