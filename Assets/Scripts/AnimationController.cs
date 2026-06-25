#nullable enable

using System.Collections;
using System.Collections.Generic;
using RtsEngine;
using RtsEngine.EntityProperties;
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
	private AnimationClip? Walk => GetAnimation("run");
	private AnimationClip? Mine => GetAnimation("interact_pickaxe");
	private AnimationClip? Retrieve => GetAnimation("run_gold");
	private AnimationClip? Attack => GetAnimation("interact_knife");

	private Sprite? Castle => GetSprite("castle");
	private Sprite? Barracks => GetSprite("barracks");

	private AnimationClip? GoldStatic => GetAnimation("static");
	private AnimationClip? GoldHighlight=> GetAnimation("highlight");


	private const float damageFlashDuration = 0.1f;

	private Animator _animator = null!;
	private SpriteRenderer _renderer = null!;

	private ColorVariant _color;
	private bool _isUnit = false;
	private bool _isStructure = false;
	private bool _isResourceNode = false;
	private int _resourceState = 0;


    void Start()
    {
		WorldStateManager.Instance.NewState += OnNewState;

		_animator = GetComponent<Animator>();
		_renderer = GetComponent<SpriteRenderer>();


		Entity? entity = WorldStateManager.Instance.GetEntity(this.gameObject);
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

	private bool TookDamage(BaseUnit unit, BaseUnit? oldUnit)
	{
		return (oldUnit != null) ? unit.HitPoints < oldUnit.HitPoints : false;
	}

	private void UpdateUnitState(BaseUnit unit)
	{
		BaseUnit? oldUnit = (BaseUnit?)WorldStateManager.Instance.GetEntityOld(unit);

		if (TookDamage(unit, oldUnit))
		{
			StartCoroutine(FlashRed());
		}

		if (unit is Worker worker)
		{
			if (worker.IsGathering)
			{
				PlayAnimation(Mine);
			}
			else if (worker.IsRetrieving) // check if retrieving but not walking as well
			{
				PlayAnimation(Retrieve);
			}
			// else if (worker.IsAttacking)
			// {
			//
			// }
			else if (unit.State.IsWalking)
			{
				PlayAnimation(Walk);
			}
			else
			{
				PlayAnimation(Idle);
			}
		}
		else if (unit is Knight knight)
		{
			// else if (worker.IsAttacking)
			// {
			//   // chance between attack1 and attack2
			// }
			if (unit.State.IsWalking)
			{
				PlayAnimation(Walk);
			}
			else
			{
				PlayAnimation(Idle);
			}
		}
	}

	private void UpdateStructureState(BaseStructure structure)
	{
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
		if (node is GoldNode)
		{
			int state = ResourceState(node, 1, 6);
			if (_resourceState != state)
			{
				LoadAssets(node);
				_resourceState = state;
			}
			PlayAnimation(GoldStatic);
		}
	}

	public void PlayAnimation(AnimationClip? clip)
	{
		if (clip == null) return;
		if (_animator == null) return;

		_animator.Play(clip.name);
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

		if (_isUnit) UpdateUnitState((BaseUnit)entity);
		else if (_isStructure) UpdateStructureState((BaseStructure)entity);
		else if (_isResourceNode) UpdateResourceNodeState((BaseResourceNode)entity);
	}

	void OnDestroy()
	{
		WorldStateManager.Instance.NewState -= OnNewState;
		DeathEffect();
	}

	private void DeathEffect()
	{
		ParticleFx.Dust1(this.transform.position);
	}
}
