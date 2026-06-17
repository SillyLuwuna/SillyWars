#nullable enable

using RtsEngine;
using RtsEngine.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
	public Tilemap tilemap = null!;
	public RuleTile groundTile = null!; 

	private bool _newConnection;

    void Start()
    {
		_newConnection = true;
		NetworkClient.Instance().ConnectionEstablished += OnConnectionEstablished;
		NetworkClient.Instance().Tick += Tick;
    }

    void Update()
    {
        
    }

	private void Tick(object? sender, WorldState state)
	{
		if (_newConnection)
		{
			UpdateTiles(state.Map);
			_newConnection = false;
		}
	}

	private void UpdateTiles(SerializableGrid<Cell> map)
	{
		Debug.Log("Updating tiles");
		tilemap.ClearAllTiles();
		for (int x = 0; x < map.Width; x++)
		{
			for (int y = 0; y < map.Height; y++)
			{
				if (map[x, y].Type == CellType.Ground)
				{
					tilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
				}
			}
		}
	}

	private void OnConnectionEstablished()
	{
		_newConnection = true;
	}
}
