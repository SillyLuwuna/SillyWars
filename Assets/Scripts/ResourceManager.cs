#nullable enable

using RtsEngine.Structures;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    void Start()
    {
		WorldStateManager.Instance.EntityUpdate += OnEntityUpdate;
		WorldStateManager.Instance.ResetState += OnReset;
		WorldStateManager.Instance.NewEntity += OnNewEntity;
		WorldStateManager.Instance.EntityDestroy += OnEntityDestroy;
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
