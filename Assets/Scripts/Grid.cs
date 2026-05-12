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

	public Vector2 CellCenter(uint x, uint y)
	{
		return new Vector2(
			_start.x + x * _strideWidth + _strideHalfsCache,
			_start.y + y * _strideWidth + _strideHalfsCache
		);
	}

	public Tuple<int, int> CellPosFromWorldSpace(Vector2 worldSpaceCoords)
	{
		int x = (int)Math.Round((worldSpaceCoords.x - (_start.x + _strideHalfsCache)) / _strideWidth);
		int y = (int)Math.Round((worldSpaceCoords.y - (_start.y + _strideHalfsCache)) / _strideWidth);
		return new Tuple<int, int>(x, y);
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
