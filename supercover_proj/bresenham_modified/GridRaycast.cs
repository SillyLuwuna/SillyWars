public class GridRaycast<T>
{
	private Grid<T> _grid;

	public GridRaycast(Grid<T> grid)
	{
		_grid = grid;
	}

	public List<Vec2Int> CastRay(Vec2 start, Vec2 end)
	{
		if (MathSelf.FloatEq(start.x, end.x) && MathSelf.FloatEq(start.y, end.y))
		{
			List<Vec2Int> collisions = new List<Vec2Int>();
			collisions.Add(_grid.CellPosFromWorldSpace(end));
			return collisions;
		}

		bool negX = false;
		bool negY = false;
		bool swap = false;

		if (MathSelf.FloatLt(end.x, start.x))
		{
			negX = true;
			end.x = -end.x;
		}

		if (MathSelf.FloatLt(end.y, start.y))
		{
			negY = true;
			end.y = -end.y;
		}

		if (MathSelf.FloatLt((end.x - start.x), (end.y - start.y)))
		{
			swap = true;
			float tmp = end.x;
			end.x = end.y;
			end.y = tmp;
		}

		return CastRayFirstOctant(start, end, negX, negY, swap);
	}

	// continuous modification of supercover line algorithm
	private List<Vec2Int> CastRayFirstOctant(Vec2 start, Vec2 end, bool negX, bool negY, bool swap)
	{
		List<Vec2Int> collisions = new List<Vec2Int>();

		Line ray = new Line(start, end);

		Vec2 curr = start;
		Vec2Int currGridPos = _grid.CellPosFromWorldSpace(curr);
		collisions.Add(currGridPos);

		while (MathSelf.FloatLte(curr.x, end.x))
		{
			float rightX = _grid.RightEdgeX(currGridPos);
			float upY = _grid.UpEdgeY(currGridPos);

			float rayY = ray.Fy(rightX);

			if (MathSelf.FloatEq(rayY, upY))
			{
				Vec2Int downRight = new Vec2Int(currGridPos.x + 1, currGridPos.y);
				Vec2Int upLeft = new Vec2Int(currGridPos.x, currGridPos.y + 1);
				collisions.Add(GetTranslatedPosition(downRight, negX, negY, swap));
				collisions.Add(GetTranslatedPosition(upLeft, negX, negY, swap));
			}

			currGridPos.x += MathSelf.FloatLte(rayY, upY) ? 1 : 0;
			currGridPos.y += MathSelf.FloatGte(rayY, upY) ? 1 : 0;

			collisions.Add(GetTranslatedPosition(currGridPos, negX, negY, swap));
			curr = _grid.WorldSpaceFromCellPos(currGridPos);
		}
		return collisions;
	}

	private Vec2Int GetTranslatedPosition(Vec2Int pos, bool negX, bool negY, bool swap)
	{
		if (negX) pos.x = -pos.x;
		if (negY) pos.y = -pos.y;
		if (swap)
		{
			int tmp = pos.x;
			pos.x = pos.y;
			pos.y = tmp;
		}
		return pos;
	}
}
