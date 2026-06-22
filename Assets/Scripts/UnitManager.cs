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
	private Dictionary<int, BaseUnit> _objectUnits = null!;

	private Dictionary<BaseUnit, Vector3> _movingUnitsGoal = null!;

	private object _updateLock = new object();
	private WorldState? _latestState;

	private bool _newConnection;

    void Start()
    {
		_newConnection = true;
		_unitInstances = new Dictionary<BaseUnit, GameObject>();
		_objectUnits = new Dictionary<int, BaseUnit>();

		_movingUnitsGoal = new Dictionary<BaseUnit, Vector3>();

		_latestState = null;

		NetworkClient.Instance().ConnectionEstablished += OnConnectionEstablished;
		NetworkClient.Instance().Tick += Tick;
    }

    void Update()
    {
		lock(_updateLock)
		{
			InterpolateUnitsTowardsGoal();

			if (_latestState == null) return;

			if (_newConnection)
			{
				Reset();
			}

			UpdateUnits(_latestState.Units);
			_latestState = null;
		}
    }

	private void InterpolateUnitsTowardsGoal()
	{
		// deltaTime is update from Update() -> Update(). We should need
		// the deltaTime also from Tick() -> Update(). To account for position resets
		float deltaTicks = Time.deltaTime / (1f / (float)NetworkClient.SERVER_TPS);

		List<BaseUnit> completed = new List<BaseUnit>();
		foreach (BaseUnit unit in _movingUnitsGoal.Keys)
		{
			GameObject unitObj = _unitInstances[unit];
			Vector3 pos = unitObj.transform.position;
			Vector3 goal = _movingUnitsGoal[unit];
			Vector3 direction = (goal - pos).normalized;
			float magnitude = unit.MoveSpeed * deltaTicks;

			if (Vector3.Distance(pos, goal) <= magnitude)
			{
				unitObj.transform.position = goal;
				completed.Add(unit);
				continue;
			}

			unitObj.transform.position += direction * magnitude;
		}

		foreach (BaseUnit unit in completed)
		{
			_movingUnitsGoal.Remove(unit);
		}
	}

	private void Tick(object? sender, WorldState state)
	{
		lock(_updateLock)
		{
			_latestState = state;
		}
	}

	private void Reset()
	{
		foreach (GameObject obj in _unitInstances.Values)
		{
			Destroy(obj);
		}
		_unitInstances.Clear();
		_objectUnits.Clear();
		_movingUnitsGoal.Clear();
		_newConnection = false;
	}

	public BaseUnit? GetUnit(GameObject obj)
	{
		int key = obj.GetInstanceID();
		if (!_objectUnits.ContainsKey(key)) return null;
		return _objectUnits[key];
	}

	private void UpdateUnits(List<BaseUnit> units)
	{
		int numUnits = units.Count;
		for(int i = 0; i < numUnits; i++)
		{
			BaseUnit unit = units[i];
			
			UpdateUnit(unit);
		}
	}

	private void UpdateUnit(BaseUnit unit)
	{
		if (unit.IsDestroyed)
		{
			DestroyUnit(unit);
			return;
		}

		Vector3 pos = new Vector3(unit.Pos.x, unit.Pos.y, unit.Pos.y);

		if (IsNewUnit(unit))
		{
			SpawnUnit(unit, pos);
			return;
		}

		UpdateWalkingGoal(unit, pos);
	}

	private void UpdateWalkingGoal(BaseUnit unit, Vector3 pos)
	{
		if (_movingUnitsGoal.ContainsKey(unit))
		{
			_unitInstances[unit].transform.position = _movingUnitsGoal[unit];
		}

		_movingUnitsGoal[unit] = pos;
	}

	private bool IsNewUnit(BaseUnit unit)
	{
		return !_unitInstances.ContainsKey(unit);
	}

	private void SpawnUnit(BaseUnit unit, Vector3 pos)
	{
		GameObject instance = Instantiate(GetCorrespondingObject(unit), pos, spawnRotation);
		_unitInstances.Add(unit, instance);
		_objectUnits.Add(instance.GetInstanceID(), unit);
	}

	private void DestroyUnit(BaseUnit unit)
	{
		Debug.Log("Removing unit");
		GameObject obj = _unitInstances[unit];
		_unitInstances.Remove(unit);
		_objectUnits.Remove(obj.GetInstanceID());
		_movingUnitsGoal.Remove(unit);
		Destroy(obj);
	}

	private GameObject GetCorrespondingObject(BaseUnit unit)
	{
		if (unit is Worker)
		{
			return WorkerUnits[unit.OwnerId];
		}
		else if (unit is Knight)
		{
			return KnightUnits[unit.OwnerId];
		}

		Debug.LogError("Unknown unit");
		return MissingTexture;
	}

	private void OnConnectionEstablished()
	{
		_newConnection = true;
	}
}
