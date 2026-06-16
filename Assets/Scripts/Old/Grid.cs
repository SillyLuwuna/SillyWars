using System;
using UnityEngine;

public class Grid<T>
{
	public uint Width => _width;
	public uint Height => _height;

	private T[] _grid;

	private Vector2 _start;
	private float _strideWidth;
	private uint _width;
	private uint _height;

	private float _strideHalfsCache;

	public Grid(Vector2 start, float strideWidth, uint width, uint height)
	{
		_start = new Vector2(start.x, start.y);
		_strideWidth = strideWidth;
		_width = width;
		_height = height;

		_grid = new T[width * height];
		_strideHalfsCache = _strideWidth / 2.0f;
	}

	public Vector2 CellCenter(int x, int y)
	{
		return new Vector2(
			_start.x + x * _strideWidth + _strideHalfsCache,
			_start.y + y * _strideWidth + _strideHalfsCache
		);
	}

	public Vector2 WorldSpaceFromCellPos(int x, int y)
	{
		return CellCenter(x, y);
	}

	public Vector2 WorldSpaceFromCellPos(Vector2Int cellPos)
	{
		return WorldSpaceFromCellPos((int)cellPos.x, (int)cellPos.y);
	}

	public Vector2Int CellPosFromWorldSpace(Vector2 worldSpaceCoords)
	{
		int x = (int)Math.Round((worldSpaceCoords.x - (_start.x + _strideHalfsCache)) / _strideWidth);
		int y = (int)Math.Round((worldSpaceCoords.y - (_start.y + _strideHalfsCache)) / _strideWidth);
		return new Vector2Int(x, y);
	}

	public Vector2Int CellPosFromWorldSpace(float x, float y)
	{
		return CellPosFromWorldSpace(new Vector2(x, y));
	}

	public bool ContainsPosFromWorldSpace(float x, float y)
	{
		if (x > _start.x + (_strideWidth * _width)) return false;
		if (x < _start.x) return false;
		if (y > _start.y + (_strideWidth * _height)) return false;
		if (y < _start.y) return false;

		return true;
	}

	public bool ContainsPosFromWorldSpace(Vector2 pos)
	{
		return ContainsPosFromWorldSpace(pos.x, pos.y);
	}

	public float RightEdgeX(Vector2Int pos)
	{
		return WorldSpaceFromCellPos(pos).x + _strideHalfsCache;
	}

	public float LeftEdgeX(Vector2Int pos)
	{
		return WorldSpaceFromCellPos(pos).x - _strideHalfsCache;
	}

	public float UpEdgeY(Vector2Int pos)
	{
		return WorldSpaceFromCellPos(pos).y + _strideHalfsCache;
	}

	public float DownEdgeY(Vector2Int pos)
	{
		return WorldSpaceFromCellPos(pos).y - _strideHalfsCache;
	}

	public T this[uint x, uint y]
	{
		get => _grid[y * _width + x];
		set => _grid[y * _width + x] = value;
	}

	public T this[int x, int y]
	{
		get => _grid[y * _width + x];
		set => _grid[y * _width + x] = value;
	}

	public void Fill(Func<T> factory)
	{
		for (int i = 0; i < _grid.Length; i++)
		{
			_grid[i] = factory();
		}
	}
}


