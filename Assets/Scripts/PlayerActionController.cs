using System.Collections.Generic;
using RtsEngine.EntityProperties;
using RtsEngine.Math;
using RtsEngine.Resources;
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
	private bool _buildCastle = false;

	void Start()
	{
		_worldStateManager.ResetState += OnReset;
	}

	public void OnRightClick(BaseUnit unit)
	{
		if (unit.OwnerId != _worldStateManager.PlayerId)
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
		if (structure.OwnerId == _worldStateManager.PlayerId)
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
		_selectedEntities = new List<Entity>() { unit };
	}

	public void OnLeftClick(BaseStructure structure)
	{
		_selectedEntities = new List<Entity>() { structure };
	}

	public void OnLeftClick(BaseResourceNode node)
	{
		_selectedEntities = new List<Entity>() { node };
	}

	public void OnLeftClick(Vec2 mousePos)
	{
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
	}

	public void OnReset()
	{
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
}
