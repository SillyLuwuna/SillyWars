#nullable enable

using RtsEngine.Structures;
using UnityEngine;

public class StructureManager : MonoBehaviour
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
		if (!(args.Entity is BaseStructure structure)) return;

		if (structure.IsBuilt)
		{
			SetTransparency(args.GameObject, 1.0f);
		}
		else
		{
			SetTransparency(args.GameObject, 0.5f);
		}
	}

	private void SetTransparency(GameObject gameObj, float alpha)
	{
		SpriteRenderer renderer = gameObj.GetComponent<SpriteRenderer>();

		Color color = renderer.color;
		color.a = alpha;

		renderer.color = color;
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
