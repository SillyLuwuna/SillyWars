using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
	[Header("Debug")]
	public bool debugShowGrid;
	public float debugCenterSize;
	public Color debugGridColor;
	public Color debugWalkableColor;

	public GameObject debugPointer;
	public bool debugShowPointerCell;

	[Header("Initialize")]
	public Tilemap mapTiles;

	private BoundsInt _tileBounds;
	private Vector2 _start;
	private float _stride;
	private uint _width;
	private uint _height;
	private Grid<Cell> _grid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		_tileBounds = AddBoundsPadding(FindTrueBounds(), 1);

		_start = new Vector2(_tileBounds.xMin, _tileBounds.yMin);
		_stride = 1.0f;
		_width = (uint)(_tileBounds.xMax - _tileBounds.xMin);
		_height = (uint)(_tileBounds.yMax - _tileBounds.yMin);

		_grid = new Grid<Cell>(_start, _stride, _width, _height);
		_grid.Fill(() => new Cell { IsWalkable = false });
		StartGrid();
    }

	private BoundsInt FindTrueBounds()
	{
		BoundsInt bounds = mapTiles.cellBounds;
		BoundsInt actualTileBounds = new BoundsInt(FirstTileInstance(), new Vector3Int(0, 0, 0));

		for (int x = bounds.xMin; x < bounds.xMax; x++)
		{
			for (int y = bounds.yMin; y < bounds.yMax; y++)
			{
				TryAddBoundsTile(ref actualTileBounds, x, y);
			}
		}

		actualTileBounds.xMax += 1;
		actualTileBounds.yMax += 1;

		return actualTileBounds;
	}

	private Vector3Int FirstTileInstance()
	{
		BoundsInt bounds = mapTiles.cellBounds;
		for (int x = bounds.xMin; x < bounds.xMax; x++)
		{
			for (int y = bounds.yMin; y < bounds.yMax; y++)
			{
				TileBase tile = mapTiles.GetTile(new Vector3Int(x, y, 0));
				if (tile != null)
				{
					return new Vector3Int(x, y, 0);
				}
			}
		}

		return new Vector3Int(0, 0, 0);
	}

	private void TryAddBoundsTile(ref BoundsInt bounds, int x, int y)
	{
		TileBase tile = mapTiles.GetTile(new Vector3Int(x, y, 0));
		if (tile != null)
		{
			if (x < bounds.xMin) bounds.xMin = x;
			if (x > bounds.xMax) bounds.xMax = x;
			if (y < bounds.yMin) bounds.yMin = y;
			if (y > bounds.yMax) bounds.yMax = y;
		}
	}

	private BoundsInt AddBoundsPadding(BoundsInt bounds, int padding)
	{
		BoundsInt paddedBounds = new BoundsInt();

		paddedBounds.xMax = bounds.xMax + padding;
		paddedBounds.yMax = bounds.yMax + padding;
		paddedBounds.xMin = bounds.xMin - padding;
		paddedBounds.yMin = bounds.yMin - padding;

		return paddedBounds;
	}

	private void StartGrid()
	{
		for (int x = _tileBounds.xMin; x < _tileBounds.xMax; x++)
		{
			for (int y = _tileBounds.yMin; y < _tileBounds.yMax; y++)
			{
				Vector3Int cellPos = new Vector3Int(x, y, 0);
				TileBase tile = mapTiles.GetTile(cellPos);
				_grid[x - _tileBounds.xMin, y - _tileBounds.yMin].IsWalkable = (tile != null);
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (debugShowGrid)
		{
			Start();
			for (uint i = 0; i < _height; i++)
			{
				for (uint j = 0; j < _width; j++)
				{
					Gizmos.color = _grid[j, i].IsWalkable ? debugWalkableColor : debugGridColor;
					Gizmos.DrawSphere(_grid.CellCenter(j, i), debugCenterSize);
				}
			}
		}

		if (debugShowPointerCell)
		{
			// Debug.Log(_grid.CellPosFromWorldSpace(debugPointer.transform.position));
			PriorityQueue<int, float> pq = new PriorityQueue<int, float>(Comparer<float>.Create((x, y) => y.CompareTo(x)));
			pq.Enqueue(4, 1.3f);
			pq.Enqueue(2, 1.2f);
			pq.Enqueue(3, 1.25f);
			pq.Enqueue(1, 0.2f);
			pq.Enqueue(5, 2.9f);
			Debug.Log(pq.Dequeue());
			Debug.Log(pq.Dequeue());
			Debug.Log(pq.Dequeue());
			Debug.Log(pq.Dequeue());
			Debug.Log(pq.Dequeue());
		}
	}

    // Update is called once per frame
    void Update()
    {
    }
}
