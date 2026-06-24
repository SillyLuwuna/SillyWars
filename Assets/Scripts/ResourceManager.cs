#nullable enable

using RtsEngine.Structures;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
	[SerializeField]
	private WorldStateManager _worldStateManager = null!;

    void Start()
    {
		_worldStateManager.EntityUpdate += OnEntityUpdate;
		_worldStateManager.ResetState += OnReset;
		_worldStateManager.NewEntity += OnNewEntity;
		_worldStateManager.EntityDestroy += OnEntityDestroy;
    }

    void Update()
    {
    }

	private void OnEntityUpdate(object? sender, EntityEventArgs args)
	{
		// if (!(args.Entity is BaseResourceNode node)) return;
	}

	private void OnReset()
	{
	}

	private void OnNewEntity(object? sender, EntityEventArgs args)
	{
	}

	private void OnEntityDestroy(object? sender, EntityEventArgs args)
	{
	}
}
