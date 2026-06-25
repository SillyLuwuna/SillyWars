#nullable enable

using RtsEngine;
using RtsEngine.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
	[SerializeField] private WorldStateManager _worldStateManager = null!;

	[SerializeField] private Tilemap groundTilemap = null!;
	[SerializeField] private Tilemap waterTilemap = null!;

	[SerializeField] private RuleTile groundTile = null!;
	[SerializeField] private AnimatedTile waterTile = null!;

	private bool _hasReset = true;

    void Start()
    {
		_worldStateManager!.ResetState += OnReset;
		_worldStateManager.NewState += OnNewState;
    }

    void Update()
    {
    }

	private void OnReset()
	{
		_hasReset = true;
	}

	private void OnNewState(object? sender, WorldState state)
	{
		if (_hasReset)
		{
			_hasReset = false;
			UpdateTiles(state.Map);
		}
	}

	private void UpdateTiles(Grid<Cell> map)
	{
		groundTilemap.ClearAllTiles();
		waterTilemap.ClearAllTiles();
		for (int x = 0; x < map.Width; x++)
		{
			for (int y = 0; y < map.Height; y++)
			{
				CellType currType = map[x, y].Type;
				if (currType == CellType.Ground || currType == CellType.Structure)
				{
					groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
					waterTilemap.SetTile(new Vector3Int(x, y, 0), waterTile);
				}
			}
		}
	}
}
