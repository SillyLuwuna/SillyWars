#nullable enable

using RtsEngine.EntityProperties;
using RtsEngine.Structures;
using UnityEngine;

public class StructureAnimationController : AnimationController<BaseStructure>
{
	private ColorVariant _color;
	private int _lastHp = -1;

	protected override string GetAssetPath(BaseStructure structure)
	{
		_color = WorldStateManager.GetColorVariant(structure.OwnerId);
		if (_color == ColorVariant.Invalid) return "unknown";

		return $"Tiny Swords/Buildings/{_color}";
	}

	protected override string GetAssetName(BaseStructure structure)
	{
		_color = WorldStateManager.GetColorVariant(structure.OwnerId);
		if (_color == ColorVariant.Invalid) return "unknown";

		if (structure is Castle)
		{
			return "Castle";
		}
		else if (structure is Barracks)
		{
			return "Barracks";
		}
		else
		{
			return "unknown";
		}
	}

    protected override void UpdateStateOverride(BaseStructure structure)
    {
		int _currHp = structure.HitPoints;

		if (_lastHp == -1)
		{
			_lastHp = _currHp;
			return;
		}

		if (_currHp < _lastHp)
		{
			StartCoroutine(FlashRed());
		}

		_lastHp = _currHp;
    }

	protected override void UpdateState(BaseStructure structure)
	{
	}


	protected override void DeathEffect()
	{
		BaseStructure entity = (BaseStructure)LastEntityUpdate!;
		Vector3 pos = transform.position;
		pos.x += (float)entity.Width / 2f;
		pos.y += (float)entity.Height / 2f;
		ParticleFx.Explosion1(pos, Mathf.Max(entity.Width, entity.Height));
	}
}
