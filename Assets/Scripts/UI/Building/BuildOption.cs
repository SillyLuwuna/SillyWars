using System;
using RtsEngine.Math;
using RtsEngine.Structures;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildOption : MonoBehaviour
{
	[SerializeField] private Image _icon;
	[SerializeField] private TMP_Text _name;
	[SerializeField] private TMP_Text _cost;

	private string _path;
	public event EventHandler<BuildOption> Pressed;
	public StructureType StructureType { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetBuilding(StructureType structureType)
	{
		StructureType = structureType;

		ColorVariant color = WorldStateManager.GetColorVariant(WorldStateManager.Instance.PlayerId);
		if (color == ColorVariant.Invalid) return;

		_path = $"Tiny Swords/Buildings/{color}";
		AssetLoader.Instance.LoadAssets(_path);

		_icon.sprite = AssetLoader.Instance.GetSprite($"{_path}/{structureType}");
		_name.text = $"{structureType}";
		_cost.text = $"{BaseStructure.FromType(structureType, null!, 0, Vec2Int.Zero).Cost.Amount}";
	}

	public void OnPress()
	{
		Pressed?.Invoke(this, this);
	}
}
