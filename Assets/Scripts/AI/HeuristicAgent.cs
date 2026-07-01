#nullable enable

using RtsEngine;
using RtsEngine.AI;
using RtsEngine.Commands;
using RtsEngine.Math;
using RtsEngine.Structures;
using RtsEngine.Units;
using UnityEngine;

public class HeuristicAgent : MonoBehaviour
{
	private RtsState? _lastState;
	private uint _playerId;
	private ulong _tick;
	private RtsActionUtils _actionUtils = null!;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

	public void Load(uint playerId)
	{
		_playerId = playerId;
		_actionUtils = new RtsActionUtils(_playerId);
		_tick = 0;
	}

	public ICommand? MakePlay(WorldState state)
	{
		_actionUtils.Update(state);
		RtsState currState = new RtsState(_lastState, state, _playerId, _tick);


		RtsAction action = RtsAction.Wait;

		if (F.Gt(currState.IdleWorkers, 0))
		{
			action = RtsAction.MineGold;
		}
		else if (F.Gt(currState.TotalUnits, 5) && F.Gt(currState.ArmyRatio, 2.0f))
		{
			action = RtsAction.Attack;
		}
		else if (F.Gt(currState.EnemyArmyValueNearBase, 0))
		{
			action = RtsAction.Defend;
		}
		else if (F.Lt(currState.Workers, 10) && F.Gte(currState.Gold, BaseUnit.Dummy(UnitType.Worker).Cost.Amount))
		{
			action = RtsAction.TrainWorker;
		}
		else if (F.Lte(currState.Barracks, 0) && F.Gte(currState.Gold, BaseStructure.FromType(StructureType.Barracks, state, _playerId, Vec2Int.Zero).Cost.Amount))
		{
			action = RtsAction.BuildBarracks;
		}
		else if (F.Lte(currState.Castles, 0) && F.Gte(currState.Gold, BaseStructure.FromType(StructureType.Castle, state, _playerId, Vec2Int.Zero).Cost.Amount))
		{
			action = RtsAction.BuildCastle;
		}
		else if (F.Gte(currState.Workers, 10) && F.Gte(currState.Gold, BaseUnit.Dummy(UnitType.Knight).Cost.Amount))
		{
			action = RtsAction.TrainKnight;
		}



		// Debug.Log($"{action}");

		_lastState = currState;
		return _actionUtils.ActionToCommand(currState, state, action);
	}
}
