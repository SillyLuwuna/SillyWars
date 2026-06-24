using System.Collections.Generic;
using RtsEngine.EntityProperties;
using RtsEngine.Math;
using RtsEngine.Structures;
using RtsEngine.Units;
using UnityEngine;

public class PlayerActionController : MonoBehaviour
{
	[SerializeField] private NetworkActionManager _networkActionManager;
	[SerializeField] private WorldStateManager _worldStateManager;

	private List<Entity> _selectedEntities = new List<Entity>();
	private bool _isWalkAttack = false;
	private bool _buildBarracks = false;

	void Start()
	{
		_worldStateManager.ResetState += OnReset;
	}

	public void OnRightClick(BaseUnit unit)
	{
		_networkActionManager.SetAggroAction(_selectedEntities, false);
		_networkActionManager.AttackAction(_selectedEntities, unit);
	}

	public void OnRightClick(BaseStructure structure)
	{
		if (structure.OwnerId == _worldStateManager.PlayerId)
		{
			_networkActionManager.BuildAction(_selectedEntities, structure);
		}
		else
		{
			_networkActionManager.AttackAction(_selectedEntities, structure);
		}
	}

	public void OnRightClick(Vec2 mousePos)
	{
		if (_isWalkAttack)
		{
			_isWalkAttack = false;
			Debug.Log($"walk attack: {_isWalkAttack}");
			_networkActionManager.SetAggroAction(_selectedEntities, true);
			_networkActionManager.MoveAction(_selectedEntities, mousePos);
		}
		else
		{
			_networkActionManager.SetAggroAction(_selectedEntities, false);
			_networkActionManager.MoveAction(_selectedEntities, mousePos);
		}
	}

	public void OnLeftClick(BaseUnit unit)
	{
		_selectedEntities = new List<Entity>() { unit };
	}

	public void OnLeftClick(BaseStructure structure)
	{
		_selectedEntities = new List<Entity>() { structure };
	}

	public void OnLeftClick(Vec2 mousePos)
	{
		if (_buildBarracks)
		{
			_buildBarracks = false;
			_networkActionManager.BuildNewAction(_selectedEntities, mousePos, StructureType.Barracks);
			Debug.Log($"build barracks: {_buildBarracks}");
		}
	}

	public void OnBuildBarracksInput()
	{
		_buildBarracks = !_buildBarracks;
		Debug.Log($"build barracks: {_buildBarracks}");
	}

	public void OnWalkAttackInput()
	{
		_isWalkAttack = !_isWalkAttack;
		Debug.Log($"walk attack: {_isWalkAttack}");
	}

	public void OnDrag(List<Entity> selectedEntities)
	{
		_selectedEntities = selectedEntities;
	}

	public void OnReset()
	{
		_selectedEntities = new List<Entity>();
		_isWalkAttack = false;
		_buildBarracks = false;
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
}
