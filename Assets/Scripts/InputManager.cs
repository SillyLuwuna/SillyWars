#nullable enable

using System.Collections.Generic;
using RtsEngine.Units;
using RtsEngine.Structures;
using UnityEngine;
using UnityEngine.InputSystem;
using RtsEngine.EntityProperties;
using RtsEngine.Resources;

public class InputManager : MonoBehaviour
{
	private const float DRAG_AMOUNT_FOR_DETECTION = 10;

	[SerializeField] private PlayerActionController _playerController = null!;
	[SerializeField] private SelectionBoxUI _selectionBoxUI = null!;

	private Vector2 _dragStart;
	private Vector2 _dragEnd;
	private bool _isDragging = false;
	private bool _isMouseClick = true;

	void Start()
	{
		WorldStateManager.Instance.ResetState += OnReset;
	}

	void Update()
	{
		if (_isDragging)
		{
			_dragEnd = Mouse.current.position.ReadValue();
			_isMouseClick = _isMouseClick && (Vector2.Distance(_dragStart, _dragEnd) < DRAG_AMOUNT_FOR_DETECTION);

			if (!_isMouseClick)
			{
				_selectionBoxUI.UpdateBox(_dragStart, _dragEnd);
			}
		}
	}

	public void OnRightClick(InputAction.CallbackContext context)
	{
		if (context.phase != InputActionPhase.Started) return;

		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		Collider2D? hit = Physics2D.OverlapPoint(mousePos);
		Entity? hitEntity = (hit?.gameObject == null) ? null : WorldStateManager.Instance.GetEntity(hit.gameObject);

		if (hitEntity == null)
		{
			_playerController.OnRightClick(WorldStateManager.Vector2ToVec2(mousePos));
		}
		else if (hitEntity is BaseUnit unit)
		{
			_playerController.OnRightClick(unit);
		}
		else if (hitEntity is BaseStructure structure)
		{
			_playerController.OnRightClick(structure);
		}
		else if (hitEntity is BaseResourceNode node)
		{
			_playerController.OnRightClick(node);
		}
	}

	public void OnLeftClick(Vector2 screenMousePos)
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(screenMousePos);
		Collider2D? hit = Physics2D.OverlapPoint(mousePos);
		Entity? hitEntity = (hit?.gameObject == null) ? null : WorldStateManager.Instance.GetEntity(hit.gameObject);

		if (hitEntity == null)
		{
			_playerController.OnLeftClick(WorldStateManager.Vector2ToVec2(mousePos));
		}
		else if (hitEntity is BaseUnit unit)
		{
			_playerController.OnLeftClick(unit);
		}
		else if (hitEntity is BaseStructure structure)
		{
			_playerController.OnLeftClick(structure);
		}
		else if (hitEntity is BaseResourceNode node)
		{
			_playerController.OnLeftClick(node);
		}
	}

	public void OnBuildCastleInput(InputAction.CallbackContext context)
	{
		if (context.phase != InputActionPhase.Started) return;

		_playerController.OnBuildCastleInput();
	}

	public void OnBuildBarracksInput(InputAction.CallbackContext context)
	{
		if (context.phase != InputActionPhase.Started) return;

		_playerController.OnBuildBarracksInput();
	}

	public void OnWalkAttackInput(InputAction.CallbackContext context)
	{
		if (context.phase != InputActionPhase.Started) return;

		_playerController.OnWalkAttackInput();
	}

	public void OnEnqueueKnightInput(InputAction.CallbackContext context)
	{
		if (context.phase != InputActionPhase.Started) return;

		_playerController.OnEnqueueKnightInput();
	}

	public void OnEnqueueWorkerInput(InputAction.CallbackContext context)
	{
		if (context.phase != InputActionPhase.Started) return;

		_playerController.OnEnqueueWorkerInput();
	}

	public void OnHaltInput(InputAction.CallbackContext context)
	{
		if (context.phase != InputActionPhase.Started) return;

		_playerController.OnHaltInput();
	}

	public void OnDrag(InputAction.CallbackContext context)
	{
		switch (context.phase)
		{
			case InputActionPhase.Started:
				OnDragStart(context);
				break;
			case InputActionPhase.Canceled:
				OnDragEnd(context);
				break;
		}
	}

	private void OnDragStart(InputAction.CallbackContext context)
	{
		_isMouseClick = true;
		_isDragging = true;
		_dragStart = Mouse.current.position.ReadValue();
		_dragEnd = _dragStart;
	}

	private void OnDragEnd(InputAction.CallbackContext context)
	{
		_isDragging = false;
		_selectionBoxUI.HideBox();

		_dragEnd = Mouse.current.position.ReadValue();
		_isMouseClick = _isMouseClick && (Vector2.Distance(_dragStart, _dragEnd) < DRAG_AMOUNT_FOR_DETECTION);


		if (_isMouseClick)
		{
			OnLeftClick(_dragEnd);
			return;
		}

		_isMouseClick = true;

		Rect selectionRect = new Rect(
			Mathf.Min(_dragStart.x, _dragEnd.x),
            Mathf.Min(_dragStart.y, _dragEnd.y),
            Mathf.Abs(_dragStart.x - _dragEnd.x),
            Mathf.Abs(_dragStart.y - _dragEnd.y)
		);

		List<GameObject> entityGameObjectsSelected = GetUnitsInRect(selectionRect);
		List<Entity> entitiesSelected = GetGameObjectEntities(entityGameObjectsSelected);
		_playerController.OnDrag(entitiesSelected);
	}

	private List<Entity> GetGameObjectEntities(List<GameObject> gameObjects)
	{
		List<Entity> entities = new List<Entity>();

		foreach (GameObject gameObj in gameObjects)
		{
			Entity? entity = WorldStateManager.Instance.GetEntity(gameObj);
			if (entity == null) continue;
			entities.Add(entity);
		}

		return entities;
	}

	public List<GameObject> GetUnitsInRect(Rect rect)
	{
		Vector3 center = Camera.main.ScreenToWorldPoint(new Vector3(
			rect.center.x,
			rect.center.y,
			0f
		));

		Vector2 start = Camera.main.ScreenToWorldPoint(rect.min);
		Vector2 end = Camera.main.ScreenToWorldPoint(rect.max);
		Vector2 overlapBox = end - start;

		Collider2D[] hits = Physics2D.OverlapBoxAll(center, overlapBox, 0f);

		List<GameObject> results = new List<GameObject>();
		foreach (var hit in hits)
		{
			results.Add(hit.gameObject);
		}

		return results;
	}

	private void OnReset()
	{
		_isDragging = false;
		_isMouseClick = true;
	}

	public void OnCameraMoveInput(InputAction.CallbackContext context)
	{
		_playerController.OnCameraMoveInput(context.ReadValue<Vector2>());
	}

	public void OnScrollInput(InputAction.CallbackContext context)
	{
		_playerController.OnScrollInput(context.ReadValue<Vector2>().y);
	}
}
