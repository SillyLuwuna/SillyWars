namespace RtsEngine;

public class GridRaycast<T>
{
	private Grid<T> _grid;

	public GridRaycast(Grid<T> grid)
	{
		_grid = grid;
	}

	public List<Vec2Int> CastRay(Vec2 start, Vec2 end, bool strictIntersections=false)
	{
		if (start == end)
		{
			List<Vec2Int> collisions = new List<Vec2Int>();
			collisions.Add(_grid.CellPosFromWorldSpace(end));
			return collisions;
		}

		bool negX = false;
		bool negY = false;
		bool swap = false;

		if (F.Lt(end.x, start.x))
		{
			negX = true;
			start.x = -start.x;
			end.x = -end.x;
		}

		if (F.Lt(end.y, start.y))
		{
			negY = true;
			start.y = -start.y;
			end.y = -end.y;
		}

		if (F.Lt((end.x - start.x), (end.y - start.y)))
		{
			swap = true;
			end.Swap();
			start.Swap();
		}

		return CastRayFirstOctant(start, end, negX, negY, swap, strictIntersections);
	}

	// continuous modification of supercover line algorithm
	private List<Vec2Int> CastRayFirstOctant(Vec2 start, Vec2 end, bool negX, bool negY, bool swap, bool strictIntersections)
	{
		List<Vec2Int> collisions = new List<Vec2Int>();

		Line ray = new Line(start, end);
		Console.WriteLine(ray);

		Vec2 curr = start;
		Vec2Int currGridPos = _grid.CellPosFromWorldSpace(curr);
		Vec2Int endGridPos = _grid.CellPosFromWorldSpace(end);
		collisions.Add(GetTranslatedPosition(currGridPos, negX, negY, swap));

		// while (F.Lt(curr.x, end.x))
		while (currGridPos.x != endGridPos.x || currGridPos.y != endGridPos.y)
		{
			float rightX = _grid.RightEdgeX(currGridPos);
			float upY = _grid.UpEdgeY(currGridPos);

			float rayY = ray.Fy(rightX);

			if (strictIntersections && F.Eq(rayY, upY))
			{
				Vec2Int downRight = new Vec2Int(currGridPos.x + 1, currGridPos.y);
				Vec2Int upLeft = new Vec2Int(currGridPos.x, currGridPos.y + 1);
				collisions.Add(GetTranslatedPosition(downRight, negX, negY, swap));
				collisions.Add(GetTranslatedPosition(upLeft, negX, negY, swap));
			}

			currGridPos.x += F.Lte(rayY, upY) ? 1 : 0;
			currGridPos.y += F.Gte(rayY, upY) ? 1 : 0;

			collisions.Add(GetTranslatedPosition(currGridPos, negX, negY, swap));
			curr = _grid.WorldSpaceFromCellPos(currGridPos);
		}
		return collisions;
	}

	private bool IsValidPos(Vec2Int pos, Vec2Int end)
	{
		return pos.x <= end.x && pos.y <= end.y;
	}

	private Vec2Int GetTranslatedPosition(Vec2Int pos, bool negX, bool negY, bool swap)
	{
		Vec2 realPos = _grid.WorldSpaceFromCellPos(pos);
		if (swap)
		{
			float tmp = realPos.x;
			realPos.x = realPos.y;
			realPos.y = tmp;
		}
		if (negX) realPos.x = -realPos.x;
		if (negY) realPos.y = -realPos.y;
		Vec2Int negPos = _grid.CellPosFromWorldSpace(realPos);
		return negPos;
	}
}
