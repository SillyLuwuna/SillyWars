#nullable enable

using RtsEngine;
using RtsEngine.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
	[SerializeField] private WorldStateManager _worldStateManager = null!;

	[SerializeField] private Tilemap tilemap = null!;
	[SerializeField] private RuleTile groundTile = null!;

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
			UpdateTiles(state.Map);
		}
	}

	private void UpdateTiles(Grid<Cell> map)
	{
		tilemap.ClearAllTiles();
		for (int x = 0; x < map.Width; x++)
		{
			for (int y = 0; y < map.Height; y++)
			{
				CellType currType = map[x, y].Type;
				if (currType == CellType.Ground || currType == CellType.Structure)
				{
					tilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
				}
			}
		}
	}
}
