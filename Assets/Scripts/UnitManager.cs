#nullable enable

using System.Collections.Generic;
using RtsEngine.Math;
using RtsEngine.Units;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
	[SerializeField]
	private WorldStateManager _worldStateManager = null!;

	private Dictionary<BaseUnit, Vector3> _movingUnitsGoal = null!;

    void Start()
    {
		_movingUnitsGoal = new Dictionary<BaseUnit, Vector3>();

		_worldStateManager.EntityUpdate += OnEntityUpdate;
		_worldStateManager.ResetState += OnReset;
		_worldStateManager.NewEntity += OnNewEntity;
		_worldStateManager.EntityDestroy += OnEntityDestroy;
    }

    void Update()
    {
		InterpolateUnitsTowardsGoal();
    }

	private void InterpolateUnitsTowardsGoal()
	{
		float deltaTicks = Time.deltaTime / (1f / (float)NetworkClient.SERVER_TPS);

		List<BaseUnit> completed = new List<BaseUnit>();
		foreach (BaseUnit unit in _movingUnitsGoal.Keys)
		{
			GameObject? unitObj = _worldStateManager.GetGameObject(unit);
			if (unitObj == null) continue;

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

	private void OnEntityUpdate(object? sender, EntityEventArgs args)
	{
		if (!(args.Entity is BaseUnit unit)) return;

		Vector3 pos = new Vector3(unit.Pos.x, unit.Pos.y, unit.Pos.y);

		UpdateWalkingGoal(unit, args.GameObject, pos);
	}

	private void UpdateWalkingGoal(BaseUnit unit, GameObject unitObj, Vector3 pos)
	{
		if (_movingUnitsGoal.ContainsKey(unit))
		{
			unitObj.transform.position = _movingUnitsGoal[unit];
		}

		if (F.Eq(Vector3.Distance(unitObj.transform.position, pos), 0f)) return;

		_movingUnitsGoal[unit] = pos;
	}

	private void OnReset()
	{
		_movingUnitsGoal.Clear();
	}

	private void OnNewEntity(object? sender, EntityEventArgs args)
	{

	}

	private void OnEntityDestroy(object? sender, EntityEventArgs args)
	{
		if (!(args.Entity is BaseUnit unit)) return;

		_movingUnitsGoal.Remove(unit);
	}
}
