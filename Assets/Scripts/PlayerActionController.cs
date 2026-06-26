using System.Collections.Generic;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using RtsEngine.Math;
using RtsEngine.Resources;
using RtsEngine.Structures;
using RtsEngine.Units;
using UnityEngine;

public class PlayerActionController : MonoBehaviour
{
	[SerializeField] private NetworkActionManager _networkActionManager;
	[SerializeField] private SelectionMenuManager _selectionMenu = null!;
	[SerializeField] private Camera _mainCamera;

	[SerializeField] private float _cameraMoveSpeed = 0f;
	[SerializeField] private float _cameraZoomSpeed = 0f;
	[SerializeField] private float _cameraMinZoom = 0f;
	[SerializeField] private float _cameraMaxZoom = 0f;
	[SerializeField] private float _cameraBound = 0f;
	private Vector3 _cameraMoveVector;

	private List<Entity> _selectedEntities = new List<Entity>();
	private bool _isWalkAttack = false;
	private bool _buildBarracks = false;
	private bool _buildCastle = false;

	void Start()
	{
		WorldStateManager.Instance.ResetState += OnReset;
	}

	void Update()
	{
		if (WorldStateManager.Instance.LatestState == null) return;

		_mainCamera.transform.position += _cameraMoveVector * _cameraMoveSpeed * Time.deltaTime * _mainCamera.orthographicSize;

		Grid<Cell> map = WorldStateManager.Instance.LatestState.Map;
		float clampedX = Mathf.Clamp(_mainCamera.transform.position.x, map.MinWorldX - _cameraBound, map.MaxWorldX + _cameraBound);
		float clampedY = Mathf.Clamp(_mainCamera.transform.position.y, map.MinWorldY - _cameraBound, map.MaxWorldY + _cameraBound);

		_mainCamera.transform.position = new Vector3(clampedX, clampedY, _mainCamera.transform.position.z);
	}

	public void OnRightClick(BaseUnit unit)
	{
		if (unit.OwnerId != WorldStateManager.Instance.PlayerId)
		{
			_networkActionManager.SetAggro(_selectedEntities, false);
			_networkActionManager.Attack(_selectedEntities, unit);
		}
		else
		{
			_networkActionManager.Move(_selectedEntities, unit.Pos);
		}
	}

	public void OnRightClick(BaseStructure structure)
	{
		if (structure.OwnerId == WorldStateManager.Instance.PlayerId)
		{
			_networkActionManager.Build(_selectedEntities, structure);
		}
		else
		{
			_networkActionManager.Attack(_selectedEntities, structure);
		}
	}

	public void OnRightClick(BaseResourceNode node)
	{
		_networkActionManager.Gather(_selectedEntities, node);
	}

	public void OnRightClick(Vec2 mousePos)
	{
		if (_selectedEntities.Count == 1)
		{
			Entity selected = _selectedEntities[0];
			if (selected is BaseStructure)
			{
				_networkActionManager.SetProductionSpawn(_selectedEntities, mousePos);
				return;
			}

		}

		if (_isWalkAttack)
		{
			_isWalkAttack = false;
			Debug.Log($"walk attack: {_isWalkAttack}");
			_networkActionManager.SetAggro(_selectedEntities, true);
			_networkActionManager.Move(_selectedEntities, mousePos);
		}
		else
		{
			_networkActionManager.SetAggro(_selectedEntities, false);
			_networkActionManager.Move(_selectedEntities, mousePos);
		}
	}

	public void OnLeftClick(BaseUnit unit)
	{
		_selectionMenu.OpenOne(unit);
		_selectedEntities = new List<Entity>() { unit };
	}

	public void OnLeftClick(BaseStructure structure)
	{
		_selectionMenu.OpenOne(structure);
		_selectedEntities = new List<Entity>() { structure };
	}

	public void OnLeftClick(BaseResourceNode node)
	{
		_selectionMenu.OpenOne(node);
		_selectedEntities = new List<Entity>() { node };
	}

	public void OnLeftClick(Vec2 mousePos)
	{
		_selectionMenu.Close();
		if (_buildBarracks)
		{
			_buildBarracks = false;
			_networkActionManager.BuildNew(_selectedEntities, mousePos, StructureType.Barracks);
			Debug.Log($"build barracks: {_buildBarracks}");
		}
		else if (_buildCastle)
		{
			_buildCastle = false;
			_networkActionManager.BuildNew(_selectedEntities, mousePos, StructureType.Castle);
			Debug.Log($"build castle: {_buildCastle}");
		}
	}

	public void OnBuildBarracksInput()
	{
		_buildCastle = false;
		_buildBarracks = !_buildBarracks;
		Debug.Log($"build barracks: {_buildBarracks}");
	}

	public void OnBuildCastleInput()
	{
		_buildBarracks = false;
		_buildCastle = !_buildCastle;
		Debug.Log($"build castle: {_buildCastle}");
	}

	public void OnWalkAttackInput()
	{
		_isWalkAttack = !_isWalkAttack;
		Debug.Log($"walk attack: {_isWalkAttack}");
	}

	public void OnEnqueueKnightInput()
	{
		if (_selectedEntities.Count != 1) return;
		Entity selected = _selectedEntities[0];

		if (!(selected is BaseStructure)) return;

		_networkActionManager.EnqueueUnitProduction(_selectedEntities, UnitType.Knight);
	}

	public void OnEnqueueWorkerInput()
	{
		if (_selectedEntities.Count != 1) return;
		Entity selected = _selectedEntities[0];

		if (!(selected is BaseStructure)) return;

		_networkActionManager.EnqueueUnitProduction(_selectedEntities, UnitType.Worker);
	}

	public void OnHaltInput()
	{
		if (_selectedEntities.Count != 1) return;
		Entity selected = _selectedEntities[0];

		_networkActionManager.Halt(_selectedEntities);
	}

	public void OnDrag(List<Entity> selectedEntities)
	{
		_selectedEntities = selectedEntities;
		_selectionMenu.Open(selectedEntities);
	}

	public void OnReset()
	{
		_selectionMenu.Close();
		_selectedEntities = new List<Entity>();
		_isWalkAttack = false;
		_buildBarracks = false;
		_buildCastle = false;
	}

	private List<uint> GetSelectedUnitIds(List<Entity> entities)
	{
		List<uint> unitIds = new List<uint>();

		foreach (Entity entity in entities)
		{
			unitIds.Add(entity.Id);
		}

		return unitIds;
	}

	public void OnCameraMoveInput(Vector2 direction)
	{
		_cameraMoveVector = ((Vector3)direction.normalized);
	}

	public void OnScrollInput(float scrollDirection)
	{
		_mainCamera.orthographicSize -= scrollDirection * _cameraZoomSpeed;
		_mainCamera.orthographicSize = Mathf.Clamp(_mainCamera.orthographicSize, _cameraMinZoom, _cameraMaxZoom);
	}

	public void OnCancel()
	{
		OnReset();
	}
}
