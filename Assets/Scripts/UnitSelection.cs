#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelection : MonoBehaviour
{
	private static UnitSelection? _instance = null;
	private static bool _awoken = false;

	[SerializeField] private SelectionBoxUI selectionBoxUI = null!;
	private Vector2 _dragStart;
	private Vector2 _dragEnd;
	// private Rect _selectionRect;
	private bool _isDragging = false;

	public List<GameObject>? UnitsSelected;

	private UnitSelection() { }

	public static UnitSelection Instance()
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

		Debug.Log("OwO");

		// SetupCollider();
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

	void Update()
	{
		if (_isDragging)
		{
			_dragEnd = Mouse.current.position.ReadValue();
			selectionBoxUI.UpdateBox(_dragStart, _dragEnd);
		}
	}
}
