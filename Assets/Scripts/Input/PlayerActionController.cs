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

	public List<Entity> SelectedEntities { get; private set; } = new List<Entity>();
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
			_networkActionManager.SetAggro(SelectedEntities, false);
			_networkActionManager.Attack(SelectedEntities, unit);
		}
		else
		{
			_networkActionManager.Move(SelectedEntities, unit.Pos);
		}
	}

	public void OnRightClick(BaseStructure structure)
	{
		if (structure.OwnerId == WorldStateManager.Instance.PlayerId)
		{
			_networkActionManager.Build(SelectedEntities, structure);
		}
		else
		{
			_networkActionManager.Attack(SelectedEntities, structure);
		}
	}

	public void OnRightClick(BaseResourceNode node)
	{
		_networkActionManager.Gather(SelectedEntities, node);
	}

	public void OnRightClick(Vec2 mousePos)
	{
		if (SelectedEntities.Count == 1)
		{
			Entity selected = SelectedEntities[0];
			if (selected is BaseStructure)
			{
				_networkActionManager.SetProductionSpawn(SelectedEntities, mousePos);
				return;
			}

		}

		if (_isWalkAttack)
		{
			_isWalkAttack = false;
			Debug.Log($"walk attack: {_isWalkAttack}");
			_networkActionManager.SetAggro(SelectedEntities, true);
			_networkActionManager.Move(SelectedEntities, mousePos);
		}
		else
		{
			_networkActionManager.SetAggro(SelectedEntities, false);
			_networkActionManager.Move(SelectedEntities, mousePos);
		}
	}

	public void OnLeftClick(BaseUnit unit)
	{
		SelectedEntities = new List<Entity>() { unit };
		_selectionMenu.Open(SelectedEntities);
	}

	public void OnLeftClick(BaseStructure structure)
	{
		SelectedEntities = new List<Entity>() { structure };
		_selectionMenu.Open(SelectedEntities);
	}

	public void OnLeftClick(BaseResourceNode node)
	{
		SelectedEntities = new List<Entity>() { node };
		_selectionMenu.Open(SelectedEntities);
	}

	public void OnLeftClick(Vec2 mousePos)
	{
		if (_buildBarracks)
		{
			_buildBarracks = false;
			_networkActionManager.BuildNew(SelectedEntities, mousePos, StructureType.Barracks);
			Debug.Log($"build barracks: {_buildBarracks}");
		}
		else if (_buildCastle)
		{
			_buildCastle = false;
			_networkActionManager.BuildNew(SelectedEntities, mousePos, StructureType.Castle);
			Debug.Log($"build castle: {_buildCastle}");
		}
		else
		{
			_selectionMenu.Close();
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
		if (SelectedEntities.Count != 1) return;
		Entity selected = SelectedEntities[0];

		if (!(selected is BaseStructure)) return;

		_networkActionManager.EnqueueUnitProduction(SelectedEntities, UnitType.Knight);
	}

	public void OnEnqueueWorkerInput()
	{
		if (SelectedEntities.Count != 1) return;
		Entity selected = SelectedEntities[0];

		if (!(selected is BaseStructure)) return;

		_networkActionManager.EnqueueUnitProduction(SelectedEntities, UnitType.Worker);
	}

	public void OnHaltInput()
	{
		if (SelectedEntities.Count != 1) return;
		Entity selected = SelectedEntities[0];

		_networkActionManager.Halt(SelectedEntities);
	}

	public void OnDrag(List<Entity> selectedEntities)
	{
		SelectedEntities = selectedEntities;
		_selectionMenu.Open(selectedEntities);
	}

	public void OnReset()
	{
		_selectionMenu.Close();
		SelectedEntities = new List<Entity>();
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
