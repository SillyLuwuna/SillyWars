#nullable enable

using System.Collections.Generic;
using RtsEngine;
using RtsEngine.Map;
using RtsEngine.Math;
using RtsEngine.Structures;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StructureManager : MonoBehaviour
{
	public GameObject[] CastleStructures = null!;
	public GameObject[] BarracksStructures = null!;
	public GameObject MissingTexture = null!;

	public Quaternion spawnRotation = Quaternion.identity;

	private Dictionary<BaseStructure, GameObject> _structureInstances = null!;
	private Dictionary<int, BaseStructure> _objectStructures = null!;

	private object _updateLock = new object();
	private WorldState? _latestState;

	private bool _newConnection;

    void Start()
    {
		_newConnection = true;
		_structureInstances = new Dictionary<BaseStructure, GameObject>();
		_objectStructures = new Dictionary<int, BaseStructure>();

		_latestState = null;

		NetworkClient.Instance().ConnectionEstablished += OnConnectionEstablished;
		NetworkClient.Instance().Tick += Tick;
    }

    void Update()
    {
		lock(_updateLock)
		{
			if (_latestState == null) return;

			if (_newConnection)
			{
				Reset();
			}

			UpdateStructures(_latestState.Structures);
			_latestState = null;
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
		foreach (GameObject obj in _structureInstances.Values)
		{
			Destroy(obj);
		}
		_structureInstances.Clear();
		_objectStructures.Clear();
		_newConnection = false;
	}

	public BaseStructure? GetStructure(GameObject obj)
	{
		int key = obj.GetInstanceID();
		if (!_objectStructures.ContainsKey(key)) return null;
		return _objectStructures[key];
	}

	private void UpdateStructures(List<BaseStructure> structures)
	{
		int numStructures = structures.Count;
		for(int i = 0; i < numStructures; i++)
		{
			BaseStructure structure = structures[i];
			
			UpdateStructure(structure);
		}
	}

	private void UpdateStructure(BaseStructure structure)
	{
		if (structure.IsDestroyed)
		{
			DestroyStructure(structure);
			return;
		}

		Vector3 pos = GetStructurePos(structure);

		if (IsNewStructure(structure))
		{
			SpawnStructure(structure, pos);
			return;
		}
	}

	private Vector3 GetStructurePos(BaseStructure structure)
	{
		float realStartY = _latestState!.Map.DownEdgeY(structure.Start);
		float realStartX = _latestState!.Map.LeftEdgeX(structure.Start);

		return new Vector3(realStartX, realStartY, realStartY);
	}

	private bool IsNewStructure(BaseStructure structure)
	{
		return !_structureInstances.ContainsKey(structure);
	}

	private void SpawnStructure(BaseStructure structure, Vector3 pos)
	{
		GameObject instance = Instantiate(GetCorrespondingObject(structure), pos, spawnRotation);
		_structureInstances.Add(structure, instance);
		_objectStructures.Add(instance.GetInstanceID(), structure);
	}

	private void DestroyStructure(BaseStructure structure)
	{
		Debug.Log("Removing structure");
		GameObject obj = _structureInstances[structure];
		_structureInstances.Remove(structure);
		_objectStructures.Remove(obj.GetInstanceID());
		Destroy(obj);
	}

	private GameObject GetCorrespondingObject(BaseStructure structure)
	{
		if (structure is Castle)
		{
			return CastleStructures[structure.OwnerId];
		}
		else if (structure is Barracks)
		{
			return BarracksStructures[structure.OwnerId];
		}

		Debug.LogError("Unknown structure");
		return MissingTexture;
	}

	private void OnConnectionEstablished()
	{
		_newConnection = true;
	}
}
