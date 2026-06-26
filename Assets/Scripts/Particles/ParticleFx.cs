#nullable enable

using System.Collections.Generic;
using UnityEngine;

public class ParticleFx : MonoBehaviour
{
	private static Dictionary<string, AnimationClip> _animations = new Dictionary<string, AnimationClip>();
	private static AnimationClip? GetParticleFx(string type) => _animations.TryGetValue(type, out var clip) ? clip : null;

	private static Dictionary<string, GameObject> _particleFxPrefabMap = new Dictionary<string, GameObject>();
	private static GameObject? GetParticleFxPrefab(string type) => _particleFxPrefabMap.TryGetValue(type, out var prefab) ? prefab : null;

	private static bool _hasLoadedParticles = false;

	public static void Dust1(Vector3 pos, float size = 1f) => Spawn(pos, "dust_1", size);
	public static void Dust2(Vector3 pos, float size = 1f) => Spawn(pos, "dust_2", size);
	public static void Explosion1(Vector3 pos, float size = 1f) => Spawn(pos, "explosion_1", size);
	public static void Explosion2(Vector3 pos, float size = 1f) => Spawn(pos, "explosion_2", size);
	public static void Fire1(Vector3 pos, float size = 1f) => Spawn(pos, "fire_1", size);
	public static void Fire2(Vector3 pos, float size = 1f) => Spawn(pos, "fire_2", size);
	public static void Fire3(Vector3 pos, float size = 1f) => Spawn(pos, "fire_3", size);
	public static void WaterSplash(Vector3 pos, float size = 1f) => Spawn(pos, "water_splash", size);

	private static bool _quitting = false;

	private void Initialize(AnimationClip? clip)
	{
		this.GetComponent<Animator>().Play(clip!.name);
		Destroy(this.gameObject, clip.length);
	}

	private static void LoadAllParticles()
	{
		string path = "Tiny Swords/Particle FX";

		Object[] animations = Resources.LoadAll(path, typeof(AnimationClip));
		foreach (Object animation in animations)
		{
			_animations.Add(AssetLoader.GetAssetString(animation.name), (AnimationClip)animation);
		}

		Object[] controllers = Resources.LoadAll(path, typeof(RuntimeAnimatorController));
		foreach (Object controller in controllers)
		{
			string name = AssetLoader.GetAssetString(controller.name);
			string animationName = $"{name}_animation";
			if (!_animations.ContainsKey(animationName))
			{
				Debug.LogError("Wrong particle fx format");
				return;
			}


			AnimationClip clip = _animations[animationName];
			_animations.Remove(animationName);
			_animations[name] = clip;

			_particleFxPrefabMap[name] = GenEffect(clip, (RuntimeAnimatorController)controller);
		}
	}

	public static GameObject GenEffect(AnimationClip clip, RuntimeAnimatorController controller)
	{
		GameObject effect = new GameObject();
		effect.SetActive(false);

		SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
		Animator animator = effect.AddComponent<Animator>();
		ParticleFx fx = effect.AddComponent<ParticleFx>();

		renderer.sortingLayerName = "Characters";
		animator.runtimeAnimatorController = controller;

		return effect;
	}


	public static void Spawn(Vector3 position, string name, float size = 1f)
	{
		if (_quitting) return;

		if (!_hasLoadedParticles)
		{
			LoadAllParticles();
			_hasLoadedParticles = true;
		}

		GameObject? prefab = GetParticleFxPrefab(name);
		if (prefab == null) return;

		GameObject effect = Instantiate(prefab, position, Quaternion.identity);
		effect.transform.localScale *= size;
		effect.SetActive(true);

		ParticleFx prefabParticleFx = effect.GetComponent<ParticleFx>();
		ParticleFx particleFx = effect.GetComponent<ParticleFx>();

		particleFx.Initialize(GetParticleFx(name));
	}


	void OnApplicationQuit()
	{
		_quitting = true;
		foreach (GameObject obj in _particleFxPrefabMap.Values)
		{
			Destroy(obj);
		}
	}
}
