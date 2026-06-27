#nullable enable

using System.Collections;
using RtsEngine;
using RtsEngine.EntityProperties;
using UnityEngine;

public abstract class AnimationController<T> : MonoBehaviour where T : Entity
{
	private const float damageFlashDuration = 0.1f;

	private Animator _animator = null!;
	private SpriteRenderer _renderer = null!;

	private string _assetPath = null!;
	private string _assetName = null!;

	protected T? LastEntityUpdate { get; private set; }
	private bool _waitAnimation = false;

	private bool _quitting = false;

	public Entity? Entity = null;


    void Start()
    {
		WorldStateManager.Instance.NewState += OnNewState;

		_animator = GetComponent<Animator>();
		_renderer = GetComponent<SpriteRenderer>();

		if (Entity == null)
		{
			Entity = WorldStateManager.Instance.GetEntity(this.gameObject);
		}

		if (Entity == null) return;
		if (!(Entity is T typedEntity)) return;
		LastEntityUpdate = typedEntity;

		_assetPath = GetAssetPath(typedEntity);
		_assetName = GetAssetName(typedEntity);

		AssetLoader.Instance.LoadAssets(_assetPath);
		LoadRenderer();

		OnNewState(this, WorldStateManager.Instance.LatestState!);
    }

	private void LoadRenderer()
	{
		RuntimeAnimatorController? controller = AssetLoader.Instance.GetAnimatorController($"{_assetPath}/{_assetName}");
		if (controller != null)
		{
			_animator.runtimeAnimatorController = controller;
		}
		else
		{
			Sprite? sprite = AssetLoader.Instance.GetSprite($"{_assetPath}/{_assetName}");
			if (sprite != null)
			{
				_renderer.sprite = sprite;
			}
		}
	}

	protected abstract string GetAssetPath(T entity);
	protected abstract string GetAssetName(T entity);

	protected IEnumerator FlashRed()
	{
		Color originalColor = _renderer.color;
		_renderer.color = Color.red;
		yield return new WaitForSeconds(damageFlashDuration);
		_renderer.color = originalColor;
	}

	protected bool TookDamage(IDestroyable destroyable, IDestroyable? oldDestroyable)
	{
		return (oldDestroyable != null) ? destroyable.HitPoints < oldDestroyable.HitPoints : false;
	}

	protected void WaitForAnimation()
	{
		_waitAnimation = true;
	}

	protected bool IsWaitingForAnimation
	{
		get
		{
			if (!_waitAnimation) return false;

			_waitAnimation = !(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
			return _waitAnimation;
		}
	}


	public void PlayAnimation(AnimationClip? clip, float speed = 1.0f)
	{
		if (clip == null) return;
		if (_animator == null) return;
		if (_animator.runtimeAnimatorController == null) return;

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

	public void FlipX(bool flip)
	{
		_renderer.flipX = flip;
	}

	public void UpdateAssets(T entity)
	{
		_assetPath = GetAssetPath(entity);
		_assetName = GetAssetName(entity);

		AssetLoader.Instance.LoadAssets(_assetPath);
		_animator.runtimeAnimatorController = AssetLoader.Instance.GetAnimatorController($"{_assetPath}/{_assetName}");
	}

	public string PathCache { get => _assetPath; }
	public string AssetNameCache { get => _assetName; }

	private void OnNewState(object? sender, WorldState state)
	{
		Entity? entity = WorldStateManager.Instance.GetEntity(this.gameObject);
		if (entity == null) return;
		if (!(entity is T typedEntity)) return;

		UpdateStateOverride(typedEntity);
		if (!IsWaitingForAnimation)
		{
			UpdateState(typedEntity);
		}
		LastEntityUpdate = typedEntity;
	}

	protected abstract void UpdateState(T entity);
	protected abstract void UpdateStateOverride(T entity);

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

	protected abstract void DeathEffect();
}
