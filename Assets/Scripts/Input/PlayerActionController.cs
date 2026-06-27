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
	[SerializeField] private SelectionMenuManager _selectionMenu = null!;
	[SerializeField] private Camera _mainCamera;

	[SerializeField] private Color _highlightColor;

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

	public bool IsWalkAttack
	{
		get => _isWalkAttack;
		set
		{
			if (value == true)
			{
				ResetState();
			}
			_isWalkAttack = value;
		}
	}

	public bool BuildBarracks
	{
		get => _buildBarracks;
		set
		{
			if (value == true)
			{
				ResetState();
			}
			_buildBarracks = value;
		}
	}

	public bool BuildCastle
	{
		get => _buildCastle;
		set
		{
			if (value == true)
			{
				ResetState();
			}
			_buildCastle = value;
		}
	}

	public List<Entity> SelectedEntities
	{
		get
		{
			return _selectedEntities;
		}
		private set
		{
			ResetEntityColor();
			_selectedEntities = value;
			HighlightEntityColor();
		}
	}

	private void ResetEntityColor()
	{
		foreach (Entity entity in SelectedEntities)
		{
			GameObject entityObj = WorldStateManager.Instance.GetGameObject(entity);
			if (entityObj == null) continue;

			SpriteRenderer renderer = entityObj.GetComponent<SpriteRenderer>();
			if (renderer == null) continue;

			renderer.color = Color.white;
		}
	}

	private void HighlightEntityColor()
	{
		foreach (Entity entity in SelectedEntities)
		{
			GameObject entityObj = WorldStateManager.Instance.GetGameObject(entity);
			if (entityObj == null) continue;

			SpriteRenderer renderer = entityObj.GetComponent<SpriteRenderer>();
			if (renderer == null) continue;

			renderer.color = _highlightColor;
		}
	}

	private void ResetState()
	{
		_isWalkAttack = false;
		_buildBarracks = false;
		_buildCastle = false;
	}

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
		_selectionMenu.OnEmptyClick();
		if (unit.OwnerId != WorldStateManager.Instance.PlayerId)
		{
			NetworkActionManager.SetAggro(SelectedEntities, false);
			NetworkActionManager.Attack(SelectedEntities, unit);
		}
		else
		{
			NetworkActionManager.Move(SelectedEntities, unit.Pos);
		}
	}

	public void OnRightClick(BaseStructure structure)
	{
		_selectionMenu.OnEmptyClick();
		if (structure.OwnerId == WorldStateManager.Instance.PlayerId)
		{
			NetworkActionManager.Build(SelectedEntities, structure);
		}
		else
		{
			NetworkActionManager.Attack(SelectedEntities, structure);
		}
	}

	public void OnRightClick(BaseResourceNode node)
	{
		_selectionMenu.OnEmptyClick();
		NetworkActionManager.Gather(SelectedEntities, node);
	}

	public void OnRightClick(Vec2 mousePos)
	{
		_selectionMenu.OnEmptyClick();
		if (SelectedEntities.Count == 1)
		{
			Entity selected = SelectedEntities[0];
			if (selected is BaseStructure)
			{
				NetworkActionManager.SetProductionSpawn(SelectedEntities, mousePos);
				return;
			}

		}

		if (IsWalkAttack)
		{
			IsWalkAttack= false;
			NetworkActionManager.SetAggro(SelectedEntities, true);
			NetworkActionManager.Move(SelectedEntities, mousePos);
		}
		else
		{
			NetworkActionManager.SetAggro(SelectedEntities, false);
			NetworkActionManager.Move(SelectedEntities, mousePos);
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
		_selectionMenu.OnEmptyClick();
		if (BuildBarracks)
		{
			BuildBarracks = false;
			NetworkActionManager.BuildNew(SelectedEntities, mousePos, StructureType.Barracks);
		}
		else if (BuildCastle)
		{
			BuildCastle = false;
			NetworkActionManager.BuildNew(SelectedEntities, mousePos, StructureType.Castle);
		}
		else
		{
			SelectedEntities = new List<Entity>();
			_selectionMenu.Close();
		}
	}

	public void OnBuildBarracksInput()
	{
		BuildBarracks = true;
	}

	public void OnBuildCastleInput()
	{
		BuildCastle = true;
	}

	public void OnWalkAttackInput()
	{
		IsWalkAttack = true;
	}

	public void OnEnqueueKnightInput()
	{
		if (SelectedEntities.Count != 1) return;
		Entity selected = SelectedEntities[0];

		if (!(selected is BaseStructure)) return;

		NetworkActionManager.EnqueueUnitProduction(SelectedEntities, UnitType.Knight);
	}

	public void OnEnqueueWorkerInput()
	{
		if (SelectedEntities.Count != 1) return;
		Entity selected = SelectedEntities[0];

		if (!(selected is BaseStructure)) return;

		NetworkActionManager.EnqueueUnitProduction(SelectedEntities, UnitType.Worker);
	}

	public void OnHaltInput()
	{
		if (SelectedEntities.Count != 1) return;
		Entity selected = SelectedEntities[0];

		NetworkActionManager.Halt(SelectedEntities);
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
		ResetState();
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
