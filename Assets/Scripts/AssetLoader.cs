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
	public Sprite? GetSprite(string path) => _spriteMap.TryGetValue(GetAssetString(path), out var sprite) ? sprite : MissingSprite;
	public Sprite? MissingSprite => _spriteMap.TryGetValue("missing_sprite", out var sprite) ? sprite : null;

	private Dictionary<string, Texture2D> _textureMap = new Dictionary<string, Texture2D>();
	public Texture2D? GetTexture(string path) => _textureMap.TryGetValue(GetAssetString(path), out var texture) ? texture: MissingTexture;
	public Texture2D? MissingTexture => _textureMap.TryGetValue("missing_texture", out var texture) ? texture : null;

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
		LoadAsset<Sprite>("missing_sprite");
		LoadAsset<Texture2D>("missing_texture");
	}

	public void LoadAsset<T>(string path) where T : UnityEngine.Object
	{
		UnityEngine.Object asset = Resources.Load<T>(path);
		CacheAsset(path, asset);
	}

	public void LoadAsset(string path)
	{
		UnityEngine.Object asset = Resources.Load(path);
		CacheAsset(path, asset);
	}

	private void CacheAsset(string path, UnityEngine.Object asset)
	{
		if (asset is AnimationClip animation)
		{
			_animationMap[GetAssetString(path)] = animation;
		}
		else if (asset is Sprite sprite)
		{
			_spriteMap[GetAssetString(path)] = sprite;
		}
		else if (asset is RuntimeAnimatorController controller)
		{
			_animatorControllerMap[GetAssetString(path)] = controller;
		}

		if (asset is Texture2D texture)
		{
			_textureMap[GetAssetString(path)] = texture;
		}
	}

	public void LoadAssets(string path)
	{
		if (_loadedPaths.Contains(path)) return;
		_loadedPaths.Add(path);

		UnityEngine.Object[] assets = Resources.LoadAll(path);
		foreach (UnityEngine.Object asset in assets)
		{
			CacheAsset($"{path}/{asset.name}", asset);
		}
	}

	public static string GetAssetString(string fileName)
	{
		return fileName.Replace(" ", "_").ToLower();
	}
}
