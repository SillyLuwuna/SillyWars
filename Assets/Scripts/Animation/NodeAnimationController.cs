#nullable enable

using RtsEngine.Resources;
using UnityEngine;

public class NodeAnimationController : AnimationController<BaseResourceNode>
{
	private AnimationClip? GoldStatic => AssetLoader.Instance.GetAnimation($"{PathCache}/{AssetNameCache}_static");
	private AnimationClip? GoldHighlight => AssetLoader.Instance.GetAnimation($"{PathCache}/{AssetNameCache}_highlight");

	private int _resourceState = 0;

	protected override string GetAssetPath(BaseResourceNode node)
	{
		string path = $"Tiny Swords/Resources/{node.Resource}/Node";
		if (node.Resource == Resource.Gold)
		{
			_resourceState = ResourceState(node, 1, 6);
			path = $"{path}/Animations{_resourceState}";
		}
		else
		{
			path = "unknown";
		}

		return path;
	}

	protected override string GetAssetName(BaseResourceNode node)
	{
		if (node.Resource == Resource.Gold)
		{
			_resourceState = ResourceState(node, 1, 6);
			return $"{node.Resource} Stone {_resourceState}";
		}
		else
		{
			return "unknown";
		}
	}

	protected override void UpdateStateOverride(BaseResourceNode node)
	{

	}

	protected override void UpdateState(BaseResourceNode node)
	{
		if (node is GoldNode)
		{
			int state = ResourceState(node, 1, 6);
			if (_resourceState != state)
			{
				UpdateAssets(node);
				_resourceState = state;
			}

			if (UnityEngine.Random.value <= 0.0055) // about 10% chance every second
			{
				PlayAnimation(GoldHighlight);
				WaitForAnimation();
			}
			else
			{
				PlayAnimation(GoldStatic);
			}
		}
	}

	private int ResourceState(BaseResourceNode node, int startState, int numStates)
	{
		int threshold = (node.MaxAmount / numStates) + 1;
		return (node.Remaining / threshold) + startState;
	}

	protected override void DeathEffect()
	{
		ParticleFx.Dust1(this.transform.position);
	}
}
