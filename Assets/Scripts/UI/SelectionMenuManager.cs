using System.Collections.Generic;
using RtsEngine;
using RtsEngine.EntityProperties;
using RtsEngine.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMenuManager : MonoBehaviour
{
	private const string _unitPortraitsPath = "Tiny Swords/UI Elements/Human Avatars";

	public GameObject SelectionMenu;
	public TMP_Text UnitHpText;
	public Image UnitSelectionPortrait;


	private List<Entity> _currEntities;
	private bool _isEnabled;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		_currEntities = new List<Entity>();
		WorldStateManager.Instance.NewState += OnNewState;
		AssetLoader.Instance.LoadAssets(_unitPortraitsPath);
		Disable();
    }

	public void OnNewState(object sender, WorldState state)
	{
		if (_currEntities == null) return;

		int offset = 0;
		for (int i = 0; i < _currEntities.Count; i++)
		{
			Entity updatedEntity = WorldStateManager.Instance.GetEntity(_currEntities[i].Id);
			if (updatedEntity == null)
			{
				offset++;
				continue;
			}
			_currEntities[i - offset] = updatedEntity;
		}
		_currEntities.RemoveRange(_currEntities.Count - offset, offset);
	}

    // Update is called once per frame
    void Update()
    {
		if (!_isEnabled) return;
		if (_currEntities.Count <= 0) return;

		if (_currEntities.Count == 1)
		{
			UpdateOne(_currEntities[0]);
		}
    }

	private void UpdateOne(Entity entity)
	{
		if (entity is BaseUnit unit)
		{
			UpdateOneUnit(unit);
		}
	}

	private void UpdateOneUnit(BaseUnit unit)
	{
		UnitHpText.text = $"{unit.HitPoints}/{unit.MaxHitPoints}";
	}

	public void Enable()
	{
		_isEnabled = true;
		SelectionMenu.SetActive(true);
	}

	public void Disable()
	{
		_currEntities.Clear();
		_isEnabled = false;
		SelectionMenu.SetActive(false);
	}

	private void OpenOne(Entity entity)
	{
		if (entity is BaseUnit unit)
		{
			OpenOneUnit(unit);
		}
	}

	private void OpenOneUnit(BaseUnit unit)
	{
		ColorVariant color = WorldStateManager.GetColorVariant(unit.OwnerId);
		UnitSelectionPortrait.sprite = AssetLoader.Instance.GetSprite($"{_unitPortraitsPath}/{color} {unit.UnitType}");
	}

	public void Open(List<Entity> entities)
	{
		if (entities.Count <= 0) return;

		Enable();
		_currEntities = entities;

		if (_currEntities.Count == 1)
		{
			OpenOne(_currEntities[0]);
			return;
		}
	}

	public void Close()
	{
		Disable();
	}
}
