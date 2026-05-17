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
			start.x = -start.x;
			end.x = -end.x;
		}

		if (MathSelf.FloatLt(end.y, start.y))
		{
			negY = true;
			start.y = -start.y;
			end.y = -end.y;
		}

		if (MathSelf.FloatLt((end.x - start.x), (end.y - start.y)))
		{
			swap = true;

			float tmp = end.x;
			end.x = end.y;
			end.y = tmp;

			tmp = start.x;
			start.x = start.y;
			start.y = tmp;
		}

		Console.WriteLine("start: " + start);
		Console.WriteLine("end: " + end);

		return CastRayFirstOctant(start, end, negX, negY, swap);
	}

	// continuous modification of supercover line algorithm
	private List<Vec2Int> CastRayFirstOctant(Vec2 start, Vec2 end, bool negX, bool negY, bool swap)
	{
		Console.WriteLine("swap: " + swap);
		Console.WriteLine("negX: " + negX);
		Console.WriteLine("negY: " + negY);
		List<Vec2Int> collisions = new List<Vec2Int>();

		// Vec2 offset = start;
		// end = new Vec2(end.x - start.x, end.y - start.y);
		// start = new Vec2(0, 0);

		Line ray = new Line(start, end);
		Console.WriteLine(ray);

		Vec2 curr = start;
		Vec2Int currGridPos = _grid.CellPosFromWorldSpace(curr);
		Vec2Int endGridPos = _grid.CellPosFromWorldSpace(end);
		collisions.Add(GetTranslatedPosition(currGridPos, negX, negY, swap));

		// while (MathSelf.FloatLt(curr.x, end.x))
		while (currGridPos.x != endGridPos.x || currGridPos.y != endGridPos.y)
		{
			float rightX = _grid.RightEdgeX(currGridPos);
			float upY = _grid.UpEdgeY(currGridPos);

			float rayY = ray.Fy(rightX);

			Console.WriteLine("rightX: " + rightX);
			Console.WriteLine("upY: " + upY);
			Console.WriteLine("rayY: " + rayY);
			Console.WriteLine("currGridPos: " + currGridPos);
			Console.WriteLine("actual: " + GetTranslatedPosition(currGridPos, negX, negY, swap));
			Console.WriteLine("curr: " + curr);
			Console.WriteLine("end: " + end);
			Console.WriteLine();

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
