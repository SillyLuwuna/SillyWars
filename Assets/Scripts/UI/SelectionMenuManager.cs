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
	private static readonly List<EntityAction> _workerAllowedActions = new List<EntityAction>() {
		EntityAction.Halt,
		EntityAction.Move,
		EntityAction.Attack,
		EntityAction.Build,
		// EntityAction.Repair,
		// EntityAction.Mine
	};
	private static readonly List<EntityAction> _knightAllowedActions = new List<EntityAction>() {
		EntityAction.Halt,
		EntityAction.Move,
		EntityAction.Attack,
	};

	[Header("General")]
	[SerializeField] private GameObject _selectionMenu;

	[Header("EntityActions")]
	[SerializeField] private Transform _actionContainer;
	[SerializeField] private GameObject _actionButtonPrefab;

	[Header("Unit UI")]
	[SerializeField] private GameObject _unitSpecificParts;
	[SerializeField] private TMP_Text _unitHpText;
	[SerializeField] private Image _unitSelectionPortrait;


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
		_unitHpText.text = $"{unit.HitPoints}/{unit.MaxHitPoints}";
	}

	public void Enable()
	{
		_isEnabled = true;
		_selectionMenu.SetActive(true);
		_unitSpecificParts.SetActive(false);
	}

	public void Disable()
	{
		_currEntities.Clear();
		_isEnabled = false;
		_selectionMenu.SetActive(false);
	}

	private void OpenOne(Entity entity)
	{
		if (entity is BaseUnit unit)
		{
			OpenOneUnit(unit);
		}

		ClearEntityActionButtons();
		SetEntityActionButtons(GetAllowedEntityActions(entity));
	}

	private void OpenOneUnit(BaseUnit unit)
	{
		_unitSpecificParts.SetActive(true);
		ColorVariant color = WorldStateManager.GetColorVariant(unit.OwnerId);
		_unitSelectionPortrait.sprite = AssetLoader.Instance.GetSprite($"{_unitPortraitsPath}/{color} {unit.UnitType}");
	}

	private List<EntityAction> GetAllowedEntityActions(Entity entity) => entity switch
	{
		Worker => _workerAllowedActions,
		Knight => _knightAllowedActions,
		_ => new List<EntityAction>(),
	};

	private void ClearEntityActionButtons()
	{
		foreach (Transform child in _actionContainer)
		{
			Destroy(child.gameObject);
		}
	}

	private void SetEntityActionButtons(List<EntityAction> allowedEntityActions)
	{
		foreach (EntityAction action in allowedEntityActions)
		{
			GameObject button = Instantiate(_actionButtonPrefab, _actionContainer);
			EntityActionButton actionButton = button.GetComponent<EntityActionButton>();
			actionButton.SetEntityAction(action);
		}
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
