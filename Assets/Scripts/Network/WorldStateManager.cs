#nullable enable

using System;
using System.Collections.Generic;
using RtsEngine;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using RtsEngine.Math;
using RtsEngine.Structures;
using RtsEngine.Units;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EntityEventArgs : EventArgs
{
	public readonly Entity Entity;
	public readonly GameObject GameObject;
	public readonly WorldState WorldState;

	public EntityEventArgs(Entity entity, GameObject gameObj, WorldState worldState) : base()
	{
		Entity = entity;
		GameObject = gameObj;
		WorldState = worldState;
	}
}

public enum ColorVariant { Blue, Red, Yellow, Purple, Black, Invalid }

public class WorldStateManager : MonoBehaviour
{
	private static WorldStateManager? _instance = null;
	private static bool _awoken = false;

	[SerializeField]
	private PrefabManager _prefabManager = null!;

	private Dictionary<Entity, Entity> _previousVersion = null!;
	private Dictionary<uint, Entity> _entitiesId = null!;
	private Dictionary<Entity, GameObject> _entityInstances = null!;
	private Dictionary<int, Entity> _objectEntities = null!;

	private object _updateLock = new object();
	private WorldState? _latestState;
	private bool _refreshState;

	private bool _newConnection;

	public event EventHandler<EntityEventArgs>? EntityUpdate;
	public event EventHandler<EntityEventArgs>? NewEntity;
	public event EventHandler<EntityEventArgs>? EntityDestroy;
	public event EventHandler<WorldState>? NewState;
	public event Action? ResetState;

	private WorldStateManager() { }

	public static WorldStateManager Instance
	{
		get
		{
			if (!_awoken || (_instance == null))
			{
				throw new MethodAccessException("Instance was not initialized yet");
			}

			return _instance;
		}
	}

	void Awake()
	{
		_instance = this;
		DontDestroyOnLoad(gameObject);
		_awoken = true;
	}

    void Start()
    {
		_newConnection = true;
		_previousVersion = new Dictionary<Entity, Entity>();
		_entitiesId = new Dictionary<uint, Entity>();
		_entityInstances = new Dictionary<Entity, GameObject>();
		_objectEntities = new Dictionary<int, Entity>();

		_latestState = null;
		_refreshState = false;

		NetworkClient.Instance.ConnectionEstablished += OnConnectionEstablished;
		NetworkClient.Instance.Tick += Tick;
    }

    void Update()
    {
		lock(_updateLock)
		{
			if (_newConnection)
			{
				Reset();
				OnResetState();
			}

			if (!_refreshState) return;
			_refreshState = false;

			UpdateEntityReferences(_latestState!); // should use a more efficient strategy
			OnNewState(_latestState!);
			UpdateEntities(_latestState!.Entities);
			RemoveEntities(_latestState!.RemovedEntities);
		}
    }

	private void Tick(object? sender, WorldState state)
	{
		lock(_updateLock)
		{
			_latestState = state;
			_refreshState = true;
		}
	}

	private void Reset()
	{
		foreach (GameObject obj in _entityInstances.Values)
		{
			Destroy(obj);
		}
		_previousVersion.Clear();
		_entitiesId.Clear();
		_entityInstances.Clear();
		_objectEntities.Clear();
		_newConnection = false;
		_refreshState = false;
		_latestState = null;
	}

	private void UpdateEntityReferences(WorldState state)
	{
		foreach (Entity entity in state.Entities)
		{
			if (!_entityInstances.ContainsKey(entity)) continue;
			int gameObj = _entityInstances[entity].GetInstanceID();

			if (_entitiesId.ContainsKey(entity.Id))
			{
				_previousVersion[entity] = _entitiesId[entity.Id];
			}

			_entitiesId[entity.Id] = entity;
			_objectEntities[gameObj] = entity;
		}
	}

	public Entity? GetEntityOld(Entity entity)
	{
		return _previousVersion.TryGetValue(entity, out Entity previous) ? previous : null;
	}

	public Entity? GetEntity(GameObject obj)
	{
		int key = obj.GetInstanceID();
		if (!_objectEntities.ContainsKey(key)) return null;
		return _objectEntities[key];
	}

	public Entity? GetEntity(uint id)
	{
		return _latestState!.GetEntity(id);
	}

	public GameObject? GetGameObject(Entity entity)
	{
		if (!_entityInstances.ContainsKey(entity)) return null;
		return _entityInstances[entity];
	}

	private void UpdateEntities(List<Entity> entities)
	{
		int numEntities = entities.Count;
		for(int i = 0; i < numEntities; i++)
		{
			Entity entity = entities[i];
			
			UpdateEntity(entity);
		}
	}

	private void UpdateEntity(Entity entity)
	{
		if (entity is IDestroyable destroyableEntity && destroyableEntity.IsDestroyed)
		{
			DestroyEntity(entity);
			OnEntityDestroy(entity);
			return;
		}

		if (IsNewEntity(entity))
		{
			SpawnEntity(entity);
			OnNewEntity(entity);
			return;
		}

		OnEntityUpdate(entity);
	}

	private bool IsNewEntity(Entity entity)
	{
		return !_entityInstances.ContainsKey(entity);
	}

	private void SpawnEntity(Entity entity)
	{
		GameObject instance = Instantiate(_prefabManager.GetCorrespondingPrefab(entity), PrefabManager.GetInstanceCoordinates(entity), Quaternion.identity);

		_entitiesId.Add(entity.Id, entity);
		_entityInstances.Add(entity, instance);
		_objectEntities.Add(instance.GetInstanceID(), entity);
	}

	private void RemoveEntities(List<uint> entitiesIds)
	{
		int numEntities = entitiesIds.Count;
		for(int i = 0; i < numEntities; i++)
		{
			DestroyEntity(entitiesIds[i]);
		}
	}

	public static Vec2 Vector2ToVec2(Vector2 vec)
	{
		return new Vec2(vec.x, vec.y);
	}

	public static Vector2 Vec2ToVector2(Vec2 vec)
	{
		return new Vector2(vec.x, vec.y);
	}

	private void DestroyEntity(Entity entity)
	{
		DestroyEntity(entity.Id);
	}

	private void DestroyEntity(uint entityId)
	{
		if (!_entitiesId.ContainsKey(entityId)) return;

		Entity entity = _entitiesId[entityId];
		GameObject entityObj = _entityInstances[entity];

		_entitiesId.Remove(entityId);
		_entityInstances.Remove(entity);
		_objectEntities.Remove(entityObj.GetInstanceID());

		Destroy(entityObj);
	}

	public WorldState? LatestState { get => _latestState; }

	public uint PlayerId { get => (uint)_latestState!.PlayerVersion; }

	private void OnConnectionEstablished()
	{
		_newConnection = true;
	}

	private void OnEntityUpdate(Entity entity)
	{
		EntityUpdate?.Invoke(this, new EntityEventArgs(entity, GetGameObject(entity)!, _latestState!));
	}

	private void OnResetState()
	{
		ResetState?.Invoke();
	}

	private void OnNewEntity(Entity entity)
	{
		NewEntity?.Invoke(this, new EntityEventArgs(entity, GetGameObject(entity)!, _latestState!));
	}

	private void OnEntityDestroy(Entity entity)
	{
		EntityDestroy?.Invoke(this, new EntityEventArgs(entity, GetGameObject(entity)!, _latestState!));
	}

	private void OnNewState(WorldState state)
	{
		NewState?.Invoke(this, state);
	}

	public static ColorVariant GetColorVariant(uint playerId)
	{
		if (Enum.GetValues(typeof(ColorVariant)).Length < playerId)
		{
			return ColorVariant.Invalid;
		}

		return (ColorVariant)playerId;
	}
}
