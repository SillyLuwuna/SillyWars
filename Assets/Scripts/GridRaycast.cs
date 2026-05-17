using System.Collections.Generic;
using UnityEngine;

public class GridRaycast<T>
{
	private Grid<T> _grid;

	public GridRaycast(Grid<T> grid)
	{
		_grid = grid;
	}

	public List<Vector2Int> CastRay(Vector2 start, Vector2 end)
	{
		if (GameMath.FloatEq(start.x, end.x) && GameMath.FloatEq(start.y, end.y))
		{
			List<Vector2Int> collisions = new List<Vector2Int>();
			collisions.Add(_grid.CellPosFromWorldSpace(end));
			return collisions;
		}

		bool negX = false;
		bool negY = false;
		bool swap = false;

		Debug.Log("start: " + start);
		Debug.Log("end: " + end);

		if (GameMath.FloatLt(end.x, start.x))
		{
			negX = true;
			start.x = -start.x;
			end.x = -end.x;
		}

		if (GameMath.FloatLt(end.y, start.y))
		{
			negY = true;
			start.y = -start.y;
			end.y = -end.y;
		}

		if (GameMath.FloatLt((end.x - start.x), (end.y - start.y)))
		{
			swap = true;

			float tmp = end.x;
			end.x = end.y;
			end.y = tmp;

			tmp = start.x;
			start.x = start.y;
			start.y = tmp;
		}

		Debug.Log("start: " + start);
		Debug.Log("end: " + end);
		Debug.Log("negX: " + negX);
		Debug.Log("negY: " + negY);
		Debug.Log("swap: " + swap);

		return CastRayFirstOctant(start, end, negX, negY, swap);
	}

	// continuous modification of supercover line algorithm
	private List<Vector2Int> CastRayFirstOctant(Vector2 start, Vector2 end, bool negX, bool negY, bool swap)
	{
		List<Vector2Int> collisions = new List<Vector2Int>();

		Line ray = new Line(start, end);

		Vector2 curr = start;
		Vector2Int currGridPos = _grid.CellPosFromWorldSpace(curr);
		collisions.Add(GetTranslatedPosition(currGridPos, negX, negY, swap));

		while (GameMath.FloatLte(curr.x, end.x))
		{
			Debug.Log(currGridPos);
			float rightX = _grid.RightEdgeX(currGridPos);
			float upY = _grid.UpEdgeY(currGridPos);

			float rayY = ray.Fy(rightX);

			if (GameMath.FloatEq(rayY, upY))
			{
				Vector2Int downRight = new Vector2Int(currGridPos.x + 1, currGridPos.y);
				Vector2Int upLeft = new Vector2Int(currGridPos.x, currGridPos.y + 1);
				collisions.Add(GetTranslatedPosition(downRight, negX, negY, swap));
				collisions.Add(GetTranslatedPosition(upLeft, negX, negY, swap));
			}

			currGridPos.x += GameMath.FloatLte(rayY, upY) ? 1 : 0;
			currGridPos.y += GameMath.FloatGte(rayY, upY) ? 1 : 0;

			collisions.Add(GetTranslatedPosition(currGridPos, negX, negY, swap));
			curr = _grid.WorldSpaceFromCellPos(currGridPos);
		}
		return collisions;
	}

	private Vector2Int GetTranslatedPosition(Vector2Int pos, bool negX, bool negY, bool swap)
	{
		Vector2 realPos = _grid.WorldSpaceFromCellPos(pos);
		if (swap)
		{
			float tmp = realPos.x;
			realPos.x = realPos.y;
			realPos.y = tmp;
		}
		if (negX) realPos.x = -realPos.x;
		if (negY) realPos.y = -realPos.y;
		Vector2Int negPos = _grid.CellPosFromWorldSpace(realPos);
		return negPos;
	}
}
