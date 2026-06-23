using System.Collections.Generic;
using RtsEngine.Math;
using RtsEngine.Map;

namespace RtsEngine.Map
{

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

		if (path.Count < 3)
		{
			return path;
		}

		int curr = 0;
		int next = 1;
		int trySkip = 2;
		optimizedPath.Add(path[curr]);

		while (trySkip < path.Count)
		{
			List<Vec2Int> collisions = _gridRaycast.CastRay(path[curr], path[trySkip], true);

			bool isSkippable = true;
			for (int j = 0; j < collisions.Count; j++)
			{
				Vec2Int collision = collisions[j];
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
				optimizedPath.Add(path[curr]);
			}
		}

		optimizedPath.Add(path.Last);

		return optimizedPath;
	}
}
}
