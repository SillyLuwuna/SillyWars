#nullable enable

using System.Collections.Generic;
using RtsEngine;
using RtsEngine.Map;
using RtsEngine.Units;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitManager : MonoBehaviour
{
	public GameObject[] WorkerUnits = null!;
	public GameObject[] KnightUnits = null!;
	public GameObject MissingTexture = null!;

	public Quaternion spawnRotation = Quaternion.identity;

	private Dictionary<BaseUnit, GameObject> _unitInstances = null!;
	private bool _newConnection;

    void Start()
    {
		_newConnection = true;
		_unitInstances = new Dictionary<BaseUnit, GameObject>();

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
			foreach (GameObject obj in _unitInstances.Values)
			{
				Destroy(obj);
			}
			_unitInstances.Clear();
			_newConnection = false;
		}

		UpdateUnits(state.GetUnitView());
	}

	private void UpdateUnits(List<BaseUnit> units)
	{
		int numUnits = units.Count;
		for(int i = 0; i < numUnits; i++)
		{
			BaseUnit unit = units[i];

			Vector3 pos = new Vector3(unit.Pos.x, unit.Pos.y, 0);

			if (!_unitInstances.ContainsKey(unit))
			{
				GameObject instance = Instantiate(GetCorrespondingObject(unit), pos, spawnRotation);
				_unitInstances.Add(unit, instance);
				continue;
			}

			_unitInstances[unit].transform.position = pos;
		}
	}

	private GameObject GetCorrespondingObject(BaseUnit unit)
	{
		switch (unit.Type)
		{
			case UnitType.Worker:
				return WorkerUnits[unit.OwnerId];
			case UnitType.Knight:
				return KnightUnits[unit.OwnerId];
			default:
				break;
		}

		Debug.LogError("Unknown unit");
		return MissingTexture;
	}

	private void OnConnectionEstablished()
	{
		_newConnection = true;
	}
}
