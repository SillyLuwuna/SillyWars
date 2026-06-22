#nullable enable

using System;
using System.Collections.Generic;
using RtsEngine;
using RtsEngine.Commands;
using RtsEngine.Math;
using RtsEngine.Units;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitController : MonoBehaviour
{
	private static UnitController? _instance = null;
	private static bool _awoken = false;

	[SerializeField] private SelectionBoxUI selectionBoxUI = null!;
	// TODO UnitManager should be singleton
	[SerializeField] private UnitManager unitManager = null!;
	private Vector2 _dragStart;
	private Vector2 _dragEnd;
	// private Rect _selectionRect;
	private bool _isDragging = false;
	private bool _isWalkAttack = false;

	public List<GameObject>? UnitsSelected;

	private UnitController() { }

	public static UnitController Instance()
	{
		if (!_awoken || _instance == null) throw new MethodAccessException("Instance was not initialized yet");
		return _instance;
	}

	void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
			return;
		}

		_instance = this;
		DontDestroyOnLoad(gameObject);
		_awoken = true;
	}

	public void OnRightClick(InputAction.CallbackContext context)
	{
		if (context.phase == InputActionPhase.Started)
		{
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
			GameObject? enemyUnit = GetEnemyUnitOnPos(mousePos);
			if (_isWalkAttack)
			{
				_isWalkAttack = false;
				SetAggroAction(true);
				MoveAction(mousePos);
			}
			else if (enemyUnit == null)
			{
				SetAggroAction(false);
				MoveAction(mousePos);
			}
			else
			{
				SetAggroAction(false);
				AttackAction(enemyUnit);
			}
		}
	}

	public GameObject? GetEnemyUnitOnPos(Vector2 pos)
	{
		Collider2D hits = Physics2D.OverlapPoint(pos);

		return hits?.gameObject;
	}

	private void MoveAction(Vector2 goal)
	{
		if (UnitsSelected == null) return;

		List<uint> unitIds = GetSelectedUnitIds();
		MoveCommandArgs args = new MoveCommandArgs(unitIds, new Vec2(goal.x, goal.y));
		ICommand command = new MoveCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	private void AttackAction(GameObject enemyUnit)
	{
		if (UnitsSelected == null) return;

		List<uint> unitIds = GetSelectedUnitIds();
		AttackCommandArgs args = new AttackCommandArgs(unitIds, unitManager.GetUnit(enemyUnit)!.Id);
		ICommand command = new AttackCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	private void SetAggroAction(bool aggro)
	{
		if (UnitsSelected == null) return;

		List<uint> unitIds = GetSelectedUnitIds();
		SetAggroCommandArgs args = new SetAggroCommandArgs(unitIds, aggro);
		ICommand command = new SetAggroCommand(0, args);
		NetworkClient.Instance().SendCommand(command);
	}

	public List<uint> GetSelectedUnitIds()
	{
		List<uint> unitIds = new List<uint>();

		foreach (GameObject obj in UnitsSelected!)
		{
			BaseUnit unit = unitManager.GetUnit(obj)!;
			unitIds.Add(unit.Id);
		}

		return unitIds;
	}

	public void OnDrag(InputAction.CallbackContext context)
	{
		switch (context.phase)
		{
			case InputActionPhase.Started:
				OnDragStart(context);
				break;
			case InputActionPhase.Performed:
				OnDragPerformed(context);
				break;
			case InputActionPhase.Canceled:
				OnDragEnd(context);
				break;
			default:
				Debug.Log("owo");
				break;
		}
	}

	private void OnDragStart(InputAction.CallbackContext context)
	{
		_isDragging = true;
		_dragStart = Mouse.current.position.ReadValue();
		_dragEnd = _dragStart;
	}

	private void OnDragPerformed(InputAction.CallbackContext context)
	{
	}

	private void OnDragEnd(InputAction.CallbackContext context)
	{
		_isDragging = false;
		_dragEnd = Mouse.current.position.ReadValue();
		Rect selectionRect = new Rect(
			Mathf.Min(_dragStart.x, _dragEnd.x),
            Mathf.Min(_dragStart.y, _dragEnd.y),
            Mathf.Abs(_dragStart.x - _dragEnd.x),
            Mathf.Abs(_dragStart.y - _dragEnd.y)
		);
		selectionBoxUI.HideBox();
		UnitsSelected = GetUnitsInRect(selectionRect);
	}

	public List<GameObject> GetUnitsInRect(Rect rect)
	{
		Vector3 center = Camera.main.ScreenToWorldPoint(new Vector3(
			rect.center.x,
			rect.center.y,
			0f
			// Camera.main.nearClipPlane + 10f
		));

		Vector2 start = Camera.main.ScreenToWorldPoint(rect.min);
		Vector2 end = Camera.main.ScreenToWorldPoint(rect.max);
		Vector2 overlapBox = end - start;

		Collider2D[] hits = Physics2D.OverlapBoxAll(center, overlapBox, 0f);

		List<GameObject> results = new List<GameObject>();
		foreach (var hit in hits)
		{
			if (hit.CompareTag("Unit"))
			{
				results.Add(hit.gameObject);
			}
		}

		return results;
	}

	public void OnWalkAttackInput(InputAction.CallbackContext context)
	{
		if (context.phase == InputActionPhase.Started)
		{
			_isWalkAttack = !_isWalkAttack;
		}
	}

	void Update()
	{
		if (_isDragging)
		{
			_dragEnd = Mouse.current.position.ReadValue();
			selectionBoxUI.UpdateBox(_dragStart, _dragEnd);
		}
	}
}
