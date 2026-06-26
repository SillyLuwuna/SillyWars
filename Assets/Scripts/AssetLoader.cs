#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class AssetLoader : MonoBehaviour
{
	private static AssetLoader? _instance = null;
	private static bool _awoken = false;

	private Dictionary<string, AnimationClip> _animationMap = new Dictionary<string, AnimationClip>();
	// private Dictionary<string, Dictionary<string, AnimationClip>> _pathAnimations = new Dictionary<string, Dictionary<string, AnimationClip>>();
	public AnimationClip? GetAnimation(string path) => _animationMap.TryGetValue(GetAssetString(path), out var clip) ? clip : null;

	private Dictionary<string, Sprite> _spriteMap = new Dictionary<string, Sprite>();
	public Sprite? GetSprite(string path) => _spriteMap.TryGetValue(GetAssetString(path), out var sprite) ? sprite : null;

	private Dictionary<string, RuntimeAnimatorController> _animatorControllerMap = new Dictionary<string, RuntimeAnimatorController>();
	public RuntimeAnimatorController? GetAnimatorController(string path) => _animatorControllerMap.TryGetValue(GetAssetString(path), out var controller) ? controller : null;

	private HashSet<string> _loadedPaths = new HashSet<string>();

	private AssetLoader() { }

	public static AssetLoader Instance
	{
		get
		{
			if (!_awoken || (_instance == null))
			{
				throw new MethodAccessException("Instance was not initialized yet");
			}

			return _instance;
		}
	}

	void Awake()
	{
		_instance = this;
		DontDestroyOnLoad(gameObject);
		_awoken = true;
	}

	public void LoadAssets(string path)
	{
		if (_loadedPaths.Contains(path)) return;
		_loadedPaths.Add(path);

		UnityEngine.Object[] assets = Resources.LoadAll(path);
		foreach (UnityEngine.Object asset in assets)
		{
			if (asset is AnimationClip animation)
			{
				// if (!_pathAnimations.ContainsKey(path))
				// {
				// 	_pathAnimations[path] = new Dictionary<string, AnimationClip>();
				// }
				//
				// _pathAnimations[$"{path}"][$"{GetAssetString(asset.name)}"] = animation;
				_animationMap[GetAssetString($"{path}/{asset.name}")] = animation;
			}
			else if (asset is Sprite sprite)
			{
				_spriteMap[GetAssetString($"{path}/{asset.name}")] = sprite;
			}
			else if (asset is RuntimeAnimatorController controller)
			{
				_animatorControllerMap[GetAssetString($"{path}/{asset.name}")] = controller;
			}
		}
	}

	public static string GetAssetString(string fileName)
	{
		// string assetString = fileName;
		//
		// int firstUnderscore = fileName.IndexOf('_');
		// int lastUnderscore = fileName.LastIndexOf('_');
		// if (firstUnderscore >= 0 && (firstUnderscore != lastUnderscore))
		// {
		// 	assetString = assetString.Substring(firstUnderscore + 1, lastUnderscore - firstUnderscore - 1);
		// }
		// else if (firstUnderscore >= 0 && (firstUnderscore == lastUnderscore))
		// {
		// 	assetString = assetString.Substring(lastUnderscore + 1, assetString.Length - lastUnderscore - 1);
		// }
		//
		// return assetString.Replace(" ", "_").ToLower();
		return fileName.Replace(" ", "_").ToLower();
	}
}
