using RtsEngine.Math;
using System;

namespace RtsEngine.Map
{

    // public class Grid<T> : ISerializable<Grid<T>> where T : new()
    public class Grid<T>
{
	public uint Width => _width;
	public uint Height => _height;

	protected T[] _grid;

	protected Vec2 _start;
	protected float _strideWidth;
	protected uint _width;
	protected uint _height;

	private float _strideHalfsCache;

	public Grid()
	{
		_grid = Array.Empty<T>();
	}

	public Grid(Vec2 start, float strideWidth, uint width, uint height) : this()
	{
		Construct(start, strideWidth, width, height);
	}

	protected void Construct(Vec2 start, float strideWidth, uint width, uint height)
	{
		_start = new Vec2(start.x, start.y);
		_strideWidth = strideWidth;
		_width = width;
		_height = height;

		_grid = new T[width * height];
		_strideHalfsCache = _strideWidth / 2.0f;
	}

	public Vec2 CellCenter(int x, int y)
	{
		return new Vec2(
			_start.x + x * _strideWidth + _strideHalfsCache,
			_start.y + y * _strideWidth + _strideHalfsCache
		);
	}

	public Vec2 WorldSpaceFromCellPos(int x, int y)
	{
		return CellCenter(x, y);
	}

	public Vec2 WorldSpaceFromCellPos(Vec2Int cellPos)
	{
		return WorldSpaceFromCellPos((int)cellPos.x, (int)cellPos.y);
	}

	public Vec2Int CellPosFromWorldSpace(Vec2 worldSpaceCoords)
	{
		int x = (int)System.Math.Round((worldSpaceCoords.x - (_start.x + _strideHalfsCache)) / _strideWidth);
		int y = (int)System.Math.Round((worldSpaceCoords.y - (_start.y + _strideHalfsCache)) / _strideWidth);
		return new Vec2Int(x, y);
	}

	public Vec2Int CellPosFromWorldSpace(float x, float y)
	{
		return CellPosFromWorldSpace(new Vec2(x, y));
	}

	public bool ContainsPosFromWorldSpace(float x, float y)
	{
		if (F.Gt(x, _start.x + (_strideWidth * _width))) return false;
		if (F.Lt(x, _start.x)) return false;
		if (F.Gt(y, _start.y + (_strideWidth * _height))) return false;
		if (F.Lt(y, _start.y)) return false;

		return true;
	}

	public bool ContainsPosFromWorldSpace(Vec2 pos)
	{
		return ContainsPosFromWorldSpace(pos.x, pos.y);
	}

	public bool ContainsPos(Vec2Int pos)
	{
		if (pos.x < 0 || pos.y < 0) return false;
		if (pos.x >= _width || pos.y >= _height) return false;
		return true;
	}

	public T GetObjectAtWorldPos(Vec2 worldPos)
	{
		Vec2Int cellPos = CellPosFromWorldSpace(worldPos);
		return this[cellPos.x, cellPos.y];
	}

	public float RightEdgeX(Vec2Int pos)
	{
		return WorldSpaceFromCellPos(pos).x + _strideHalfsCache;
	}

	public float LeftEdgeX(Vec2Int pos)
	{
		return WorldSpaceFromCellPos(pos).x - _strideHalfsCache;
	}

	public float UpEdgeY(Vec2Int pos)
	{
		return WorldSpaceFromCellPos(pos).y + _strideHalfsCache;
	}

	public float DownEdgeY(Vec2Int pos)
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

	public T this[int i]
	{
		get => _grid[i];
		set => _grid[i] = value;
	}

	public void Fill(Func<T> factory)
	{
		for (int i = 0; i < _grid.Length; i++)
		{
			_grid[i] = factory();
		}
	}

	public uint Size()
	{
		return _width * _height;
	}

	public float MaxWorldX
	{
		get => _start.x + _width;
	}

	public float MaxWorldY
	{
		get => _start.y + _height;
	}

	public float MinWorldX
	{
		get => _start.x;
	}

	public float MinWorldY
	{
		get => _start.y;
	}

}

}
