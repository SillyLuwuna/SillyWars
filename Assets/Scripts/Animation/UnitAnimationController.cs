#nullable enable

using RtsEngine.Math;
using RtsEngine.Units;
using UnityEngine;

public class UnitAnimationController : AnimationController<BaseUnit>
{
	private AnimationClip? Idle => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_idle_{_suffix}");
	private AnimationClip? IdleAxe => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_idle_axe_{_suffix}");
	private AnimationClip? IdleGold => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_idle_gold_{_suffix}");
	private AnimationClip? IdleHammer => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_idle_hammer_{_suffix}");
	private AnimationClip? IdleKnife => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_idle_knife_{_suffix}");
	private AnimationClip? IdleMeat => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_idle_meat_{_suffix}");
	private AnimationClip? IdlePickaxe => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_idle_pickaxe_{_suffix}");
	private AnimationClip? IdleWood => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_idle_wood_{_suffix}");

	private AnimationClip? Walk => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_run_{_suffix}");
	private AnimationClip? WalkGold => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_run_gold_{_suffix}");
	private AnimationClip? WalkAxe => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_run_axe_{_suffix}");
	private AnimationClip? WalkHammer => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_run_hammer_{_suffix}");
	private AnimationClip? WalkKnife => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_run_knife_{_suffix}");
	private AnimationClip? WalkMeat => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_run_meat_{_suffix}");
	private AnimationClip? WalkPickaxe => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_run_pickaxe_{_suffix}");
	private AnimationClip? WalkWood => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_run_wood_{_suffix}");

	private AnimationClip? Mine => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_interact_pickaxe_{_suffix}");
	private AnimationClip? Chop => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_interact_axe_{_suffix}");
	private AnimationClip? Build => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_interact_hammer_{_suffix}");

	private AnimationClip? WorkerAttack => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_interact_knife_{_suffix}");
	private AnimationClip? KnightAttack1 => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_attack1_{_suffix}");
	private AnimationClip? KnightAttack2 => AssetLoader.Instance.GetAnimation($"{PathCache}/{_prefix}_attack2_{_suffix}");

	private ColorVariant _color;
	private string _prefix = null!;
	private string _suffix = null!;

	private int _lastHp = -1;

	protected override string GetAssetPath(BaseUnit unit)
	{
		_color = WorldStateManager.GetColorVariant(unit.OwnerId);
		if (_color == ColorVariant.Invalid) return "unknown";

		return $"Tiny Swords/Units/{_color}/{unit.UnitType}/Animations";
	}

	protected override string GetAssetName(BaseUnit unit)
	{
		_color = WorldStateManager.GetColorVariant(unit.OwnerId);
		if (_color == ColorVariant.Invalid) return "unknown";

		_suffix = _color.ToString();
		if (unit is Worker)
		{
			_prefix = "Pawn";
		}
		else if (unit is Knight)
		{
			_prefix = "Warrior";
		}
		else
		{
			_prefix = "unknown";
		}

		return $"{_prefix}_{_suffix}";
	}

	protected override void UpdateStateOverride(BaseUnit unit)
	{
		int _currHp = unit.HitPoints;

		if (_lastHp == -1)
		{
			_lastHp = _currHp;
		}
		else
		{
			if (_currHp < _lastHp)
			{
				StartCoroutine(FlashRed());
			}

			_lastHp = _currHp;
		}

		Vec2? goal = unit.NextWaypoint;
		if (goal != null)
		{
			Vec2 direction = unit.Pos.To(goal.Value);
			FlipX(direction.x <= 0);
		}

	}

	protected override void UpdateState(BaseUnit unit)
	{
		if (unit is Worker worker)
		{
			WorkerAnimations(worker);
		}
		else if (unit is Knight knight)
		{
			KnightAnimations(knight);
		}
	}

	private void WorkerAnimations(Worker worker)
	{
		if (worker.IsGathering)
		{
			PlayAnimation(Mine);
		}
		else if (worker.IsRetrieving) // check if retrieving but not walking as well
		{
			PlayAnimation(WalkGold);
		}
		else if (worker.Attacked)
		{
			PlayAnimation(WorkerAttack, 1f / ((float)worker.AttackSpeed / (float)NetworkClient.SERVER_TPS));
			WaitForAnimation();
		}
		else if (worker.IsBuilding)
		{
			PlayAnimation(Build);
		}
		else if (worker.State.IsWalking)
		{
			PlayAnimation(Walk);
		}
		else
		{
			PlayAnimation(Idle);
		}
	}

	private void KnightAnimations(Knight knight)
	{
		if (knight.Attacked)
		{
			if (UnityEngine.Random.value > 0.5f)
			{
				PlayAnimation(KnightAttack1, 1f / ((float)knight.AttackSpeed / (float)NetworkClient.SERVER_TPS));
			}
			else
			{
				PlayAnimation(KnightAttack2, 1f / ((float)knight.AttackSpeed / (float)NetworkClient.SERVER_TPS));
			}
			WaitForAnimation();
		}
		else if (knight.State.IsWalking)
		{
			PlayAnimation(Walk);
		}
		else
		{
			PlayAnimation(Idle);
		}
	}

	protected override void DeathEffect()
	{
		ParticleFx.Dust1(this.transform.position);
	}
}
