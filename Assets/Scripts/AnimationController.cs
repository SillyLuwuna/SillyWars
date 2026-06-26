#nullable enable

using System.Collections;
using System.Collections.Generic;
using RtsEngine;
using RtsEngine.EntityProperties;
using RtsEngine.Math;
using RtsEngine.Resources;
using RtsEngine.Structures;
using RtsEngine.Units;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
	private Dictionary<string, AnimationClip> _animationMap = new Dictionary<string, AnimationClip>();
	private AnimationClip? GetAnimation(string type) => _animationMap.TryGetValue(type, out var clip) ? clip : null;

	private Dictionary<string, Sprite> _spriteMap = new Dictionary<string, Sprite>();
	private Sprite? GetSprite(string type) => _spriteMap.TryGetValue(type, out var sprite) ? sprite : null;


	private AnimationClip? Idle => GetAnimation("idle");
	private AnimationClip? IdleAxe => GetAnimation("idle_axe");
	private AnimationClip? IdleGold => GetAnimation("idle_gold");
	private AnimationClip? IdleHammer => GetAnimation("idle_hammer");
	private AnimationClip? IdleKnife => GetAnimation("idle_knife");
	private AnimationClip? IdleMeat => GetAnimation("idle_meat");
	private AnimationClip? IdlePickaxe => GetAnimation("idle_pickaxe");
	private AnimationClip? IdleWood => GetAnimation("idle_wood");

	private AnimationClip? Walk => GetAnimation("run");
	private AnimationClip? WalkGold => GetAnimation("run_gold");
	private AnimationClip? WalkAxe => GetAnimation("run_axe");
	private AnimationClip? WalkHammer => GetAnimation("run_hammer");
	private AnimationClip? WalkKnife => GetAnimation("run_knife");
	private AnimationClip? WalkMeat => GetAnimation("run_meat");
	private AnimationClip? WalkPickaxe => GetAnimation("run_pickaxe");
	private AnimationClip? WalkWood => GetAnimation("run_wood");

	private AnimationClip? Mine => GetAnimation("interact_pickaxe");
	private AnimationClip? Chop => GetAnimation("interact_axe");
	private AnimationClip? Build => GetAnimation("interact_hammer");

	private AnimationClip? WorkerAttack => GetAnimation("interact_knife");
	private AnimationClip? KnightAttack1 => GetAnimation("attack1");
	private AnimationClip? KnightAttack2 => GetAnimation("attack2");




	private Sprite? Castle => GetSprite("castle");
	private Sprite? Barracks => GetSprite("barracks");

	private AnimationClip? GoldStatic => GetAnimation("static");
	private AnimationClip? GoldHighlight=> GetAnimation("highlight");


	private const float damageFlashDuration = 0.1f;

	private Animator _animator = null!;
	private SpriteRenderer _renderer = null!;

	private Entity? LastEntityUpdate;
	private ColorVariant _color;
	private bool _isUnit = false;
	private bool _isStructure = false;
	private bool _isResourceNode = false;
	private int _resourceState = 0;
	private bool _waitAnimation = false;

	private bool _quitting = false;


    void Start()
    {
		WorldStateManager.Instance.NewState += OnNewState;

		_animator = GetComponent<Animator>();
		_renderer = GetComponent<SpriteRenderer>();


		Entity? entity = WorldStateManager.Instance.GetEntity(this.gameObject);
		LastEntityUpdate = entity;
		if (entity == null) return;

		if (entity is BaseUnit) _isUnit = true;
		else if (entity is BaseStructure) _isStructure = true;
		else if (entity is BaseResourceNode) _isResourceNode = true;

		_color = WorldStateManager.GetColorVariant(entity.OwnerId);
		if ((_isUnit || _isStructure) && (_color == ColorVariant.Invalid)) return;

		LoadAssets(entity);

		OnNewState(this, WorldStateManager.Instance.LatestState!);
    }

	private void LoadAssets(Entity entity)
	{

		string path = $"Tiny Swords";
		if (entity is BaseUnit unit)
		{
			LoadAssets($"{path}/Units/{_color}/{unit.UnitType}/Animations");
		}
		else if (entity is BaseStructure structure)
		{
			LoadAssets($"{path}/Buildings/{_color}");
		}
		else if (entity is BaseResourceNode node)
		{
			path = $"{path}/Resources/{node.Resource}/Node";
			if (node.Resource == Resource.Gold)
			{
				_resourceState = ResourceState(node, 1, 6);
				LoadAssets($"{path}/Animations{_resourceState}");
			}
		}
		else
		{
			return;
		}
	}

	private int ResourceState(BaseResourceNode node, int startState, int numStates)
	{
		int threshold = (node.MaxAmount / numStates) + 1;
		return (node.Remaining / threshold) + startState;
	}

	private void LoadAssets(string path)
	{
		Object[] assets = Resources.LoadAll(path);
		foreach (Object asset in assets)
		{
			if (asset is AnimationClip animation)
			{
				_animationMap[GetAssetString(asset.name)] = animation;
			}
			else if (asset is Sprite sprite)
			{
				_spriteMap[GetAssetString(asset.name)] = sprite;
			}
			else if (asset is RuntimeAnimatorController controller)
			{
				_animator.runtimeAnimatorController = controller;
			}
		}
	}

	public static string GetAssetString(string fileName)
	{
		string assetString = fileName;

		int firstUnderscore = fileName.IndexOf('_');
		int lastUnderscore = fileName.LastIndexOf('_');
		if (firstUnderscore >= 0 && (firstUnderscore != lastUnderscore))
		{
			assetString = assetString.Substring(firstUnderscore + 1, lastUnderscore - firstUnderscore - 1);
		}
		else if (firstUnderscore >= 0 && (firstUnderscore == lastUnderscore))
		{
			assetString = assetString.Substring(lastUnderscore + 1, assetString.Length - lastUnderscore - 1);
		}

		return assetString.Replace(" ", "_").ToLower();
	}

	private IEnumerator FlashRed()
	{
		Color originalColor = _renderer.color;
		_renderer.color = Color.red;
		yield return new WaitForSeconds(damageFlashDuration);
		_renderer.color = originalColor;
	}

	private bool TookDamage(IDestroyable destroyable, IDestroyable? oldDestroyable)
	{
		return (oldDestroyable != null) ? destroyable.HitPoints < oldDestroyable.HitPoints : false;
	}

	private void WaitForAnimation()
	{
		_waitAnimation = true;
	}

	private bool IsWaitingForAnimation
	{
		get
		{
			if (!_waitAnimation) return false;

			_waitAnimation = !(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
			return _waitAnimation;
		}
	}

	private void UpdateUnitState(BaseUnit unit)
	{
		BaseUnit? oldUnit = (BaseUnit?)WorldStateManager.Instance.GetEntityOld(unit);

		if (TookDamage(unit, oldUnit))
		{
			StartCoroutine(FlashRed());
		}

		Vec2? goal = unit.NextWaypoint;
		if (goal != null)
		{
			Vec2 direction = unit.Pos.To(goal.Value);
			_renderer.flipX = (direction.x <= 0);
		}
		else
		{
			_renderer.flipX = true;
		}

		if (IsWaitingForAnimation) return;

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

	private void UpdateStructureState(BaseStructure structure)
	{
		BaseStructure? oldStructure = (BaseStructure?)WorldStateManager.Instance.GetEntityOld(structure);

		if (TookDamage(structure, oldStructure))
		{
			StartCoroutine(FlashRed());
		}

		if (IsWaitingForAnimation) return;

		if (structure is Castle)
		{
			SetSprite(Castle);
		}
		else if (structure is Barracks)
		{
			SetSprite(Barracks);
		}
	}

	private void UpdateResourceNodeState(BaseResourceNode node)
	{
		if (IsWaitingForAnimation) return;

		if (node is GoldNode)
		{
			int state = ResourceState(node, 1, 6);
			if (_resourceState != state)
			{
				LoadAssets(node);
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

	public void PlayAnimation(AnimationClip? clip, float speed = 1.0f)
	{
		if (clip == null) return;
		if (_animator == null) return;

		_animator.Play(clip.name);
		_animator.speed = speed;
	}

	public void SetSprite(Sprite? sprite)
	{
		if (sprite == null) return;
		if (_renderer == null) return;
		if (_renderer.sprite == sprite) return;

		_renderer.sprite = sprite;
	}

	private void OnNewState(object? sender, WorldState state)
	{
		Entity? entity = WorldStateManager.Instance.GetEntity(this.gameObject);
		if (entity == null) return;

		LastEntityUpdate = entity;
		if (_isUnit) UpdateUnitState((BaseUnit)entity);
		else if (_isStructure) UpdateStructureState((BaseStructure)entity);
		else if (_isResourceNode) UpdateResourceNodeState((BaseResourceNode)entity);
	}

	void OnApplicationQuit()
	{
		_quitting = true;
	}

	void OnDestroy()
	{
		if (_quitting) return;

		WorldStateManager.Instance.NewState -= OnNewState;
		DeathEffect();
	}

	private void DeathEffect()
	{
		if (_isUnit)
		{
			ParticleFx.Dust1(this.transform.position);
		}
		else if (_isStructure)
		{
			BaseStructure entity = (BaseStructure)LastEntityUpdate!;
			Vector3 pos = transform.position;
			pos.x += (float)entity.Width / 2f;
			pos.y += (float)entity.Height / 2f;
			ParticleFx.Explosion1(pos, Mathf.Max(entity.Width, entity.Height));
		}
		else if (_isResourceNode)
		{
			ParticleFx.Dust1(this.transform.position);
		}
	}
}
