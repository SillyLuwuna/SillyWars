using System;
using System.Collections.Generic;
using RtsEngine;
using RtsEngine.EntityProperties;
using RtsEngine.Structures;
using RtsEngine.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMenuManager : MonoBehaviour
{
	private const string _unitPortraitsPath = "Tiny Swords/UI Elements/Human Avatars";
	private const string _structurePath = "Tiny Swords/Buildings";

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

	private static readonly List<EntityAction> _castleAllowedActions = new List<EntityAction>()
	{
		EntityAction.EnqueueWorker,
	};

	private static readonly List<EntityAction> _barracksAllowedActions = new List<EntityAction>()
	{
		EntityAction.EnqueueKnight,
	};

	[Header("General")]
	[SerializeField] private GameObject _selectionMenu;
	[SerializeField] private PlayerActionController _playerActionController;

	[Header("EntityActions")]
	[SerializeField] private Transform _actionContainer;
	[SerializeField] private GameObject _actionButtonPrefab;

	[Header("Unit UI")]
	[SerializeField] private GameObject _unitSpecificParts;
	[SerializeField] private TMP_Text _unitHpText;
	[SerializeField] private Image _unitSelectionPortrait;

	[Header("Build UI")]
	[SerializeField] private Transform _buildOptionsContainer;
	[SerializeField] private Transform _buildOptionPrefab;
	[SerializeField] private BuildHelper _buildHelper;

	[Header("Structure UI")]
	[SerializeField] private TrainQueueManager _trainQueueManager;



	private List<Entity> _currEntities = new List<Entity>();
	private bool _isEnabled;

	private bool _firstState = true;
	private bool _firstBuild = true;

	void Awake()
	{
		this.gameObject.SetActive(false);
	}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		WorldStateManager.Instance.NewState += OnNewState;
		WorldStateManager.Instance.ResetState += OnReset;
		AssetLoader.Instance.LoadAssets(_unitPortraitsPath);
		OnReset();
    }

	private void OnReset()
	{
		_firstState = false;
		_buildOptionsContainer.gameObject.SetActive(false);
		_buildHelper.Close();
		Disable();
	}

	private void InitBuildOptions()
	{
		if (!_firstBuild) return;
		_firstBuild = false;

		ClearBuildOptions();
		_buildOptionsContainer.gameObject.SetActive(false);
		foreach (StructureType structureType in Enum.GetValues(typeof(StructureType)))
		{
			GameObject button = Instantiate(_buildOptionPrefab.gameObject, _buildOptionsContainer);
			BuildOption buildOption = button.GetComponent<BuildOption>();
			buildOption.SetBuilding(structureType);

			buildOption.Pressed += OnBuildOptionButtonPressed;
		}
	}

	private void ClearBuildOptions()
	{
		foreach (Transform child in _buildOptionsContainer)
		{
			Destroy(child.gameObject);
		}
	}

	public void OnNewState(object sender, WorldState state)
	{
		if (_firstState)
		{
			_firstState = false;
			Debug.Log("Init");
			InitBuildOptions();
		}

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
		if (entity is IDestroyable destroyable)
		{
			UpdateHPText(destroyable);
		}
	}

	private void UpdateHPText(IDestroyable destroyable)
	{
		_unitHpText.text = $"{destroyable.HitPoints}/{destroyable.MaxHitPoints}";
	}

	public void Enable()
	{
		_isEnabled = true;
		// _selectionMenu.SetActive(true);
		_unitSpecificParts.SetActive(false);
		_buildOptionsContainer.gameObject.SetActive(false);
		_trainQueueManager.Close();
		_buildHelper.Close();
	}

	public void Disable()
	{
		ClearEntityActionButtons();
		// ClearBuildOptions();
		_buildOptionsContainer.gameObject.SetActive(false);
		_currEntities = new List<Entity>();
		_isEnabled = false;
		_buildHelper.Close();
		// _selectionMenu.SetActive(false);
	}

	private void OpenOne(Entity entity)
	{
		_unitSpecificParts.SetActive(true);
		uint playerId = WorldStateManager.Instance.PlayerId;

		if (entity is BaseUnit unit)
		{
			SetUnitPortrait(unit);
			if (entity.OwnerId == playerId)
			{
				OpenOneUnit(unit);
			}
		}
		else if (entity is UnitProducer structure)
		{
			SetStructurePortrait(structure);
			if (entity.OwnerId == playerId)
			{
				OpenOneStructure(structure);
			}
		}

		ClearEntityActionButtons();
		if (entity.OwnerId == playerId)
		{
			SetEntityActionButtons(entity);
		}
	}

	private void SetPortrait(Sprite sprite)
	{
		_unitSelectionPortrait.sprite = sprite;
	}

	private void SetUnitPortrait(BaseUnit unit)
	{
		ColorVariant color = WorldStateManager.GetColorVariant(unit.OwnerId);
		SetPortrait(AssetLoader.Instance.GetSprite($"{_unitPortraitsPath}/{color} {unit.UnitType}"));
	}

	private void OpenOneUnit(BaseUnit unit)
	{

	}

	private void SetStructurePortrait(BaseStructure structure)
	{
		ColorVariant color = WorldStateManager.GetColorVariant(structure.OwnerId);

		AssetLoader.Instance.LoadAssets($"{_structurePath}/{color}");

		SetPortrait(AssetLoader.Instance.GetSprite($"{_structurePath}/{color}/{structure.StructureType}"));
	}

	private void OpenOneStructure(UnitProducer structure)
	{
		_trainQueueManager.Open(structure);
	}

	private List<EntityAction> GetAllowedEntityActions(Entity entity) => entity switch
	{
		Worker => _workerAllowedActions,
		Knight => _knightAllowedActions,
		Castle => _castleAllowedActions,
		Barracks => _barracksAllowedActions,
		_ => new List<EntityAction>(),
	};

	private void ClearEntityActionButtons()
	{
		foreach (Transform child in _actionContainer)
		{
			Destroy(child.gameObject);
		}
	}

	private void SetEntityActionButtons(Entity entity)
	{
		List<EntityAction> allowedEntityActions = GetAllowedEntityActions(entity);
		ColorVariant color = WorldStateManager.GetColorVariant(entity.OwnerId);

		foreach (EntityAction action in allowedEntityActions)
		{
			GameObject button = Instantiate(_actionButtonPrefab, _actionContainer);
			EntityActionButton actionButton = button.GetComponent<EntityActionButton>();
			actionButton.SetEntityAction(_playerActionController, action, color);
			actionButton.Pressed += OnActionButtonPressed;
		}
	}

	public void OnActionButtonPressed(object sender, EntityActionButton action)
	{
		if (action.Action == EntityAction.Build)
		{
			InitBuildOptions();
			_buildOptionsContainer.gameObject.SetActive(true);
		}
		else
		{
			_buildOptionsContainer.gameObject.SetActive(false);
		}
	}

	public void OnBuildOptionButtonPressed(object sender, BuildOption buildOption)
	{
		_buildHelper.Close();
		_buildHelper.Open(buildOption.StructureType);

		switch (buildOption.StructureType)
		{
			case StructureType.Castle:
				_playerActionController.BuildCastle = true;
				break;
			case StructureType.Barracks:
				_playerActionController.BuildBarracks = true;
				break;
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

	public void OnEmptyClick()
	{
		_buildHelper.Close();
	}
}
