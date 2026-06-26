using System.Collections.Generic;
using RtsEngine;
using RtsEngine.EntityProperties;
using RtsEngine.Units;
using TMPro;
using UnityEngine;

public class SelectionMenuManager : MonoBehaviour
{
	public GameObject SelectionMenu;
	public TMP_Text UnitHpText;


	private List<Entity> _currEntities;
	private bool _isEnabled;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		_currEntities = new List<Entity>();
		WorldStateManager.Instance.NewState += OnNewState;
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

	public void Open(List<Entity> entities)
	{
		Enable();
		_currEntities = entities;
	}

	public void OpenOne(Entity entity)
	{
		Open(new List<Entity>() { entity });
	}

	public void Close()
	{
		Disable();
	}
}
