using System.Linq;
using RtsEngine.Commands;
using RtsEngine.EntityProperties;
using RtsEngine.Math;
using RtsEngine.Resources;
using RtsEngine.Structures;
using RtsEngine.Units;

namespace RtsEngine.AI
{

public enum RtsAction
{
	TrainWorker,
	TrainKnight,

	BuildBarracks,
	BuildCastle,

	Attack,
	Defend,

	MineGold,

	Wait,
}

public class RtsActionUtils
{
	// private List<Castle> _castles;
	// private int _castleIterator;
	// private List<Barracks> _barracks;
	// private int _barracksIterator;
	//
	// private List<Castle> _castlesClean;
	// private List<Barracks> _barracksClean;

	// private List<Vec2Int> _validBuildLocations;

	private uint _playerId;

	public RtsActionUtils(uint playerId)
	{
		_playerId = playerId;

		// _castlePool = new HashSet<Castle>();
		// _barracksPool = new HashSet<Barracks>();

		// _castles = new List<Castle>();
		// _castlesClean = new List<Castle>();
		// _castleIterator = 0;
		//
		// _barracks = new List<Barracks>();
		// _barracksClean = new List<Barracks>();
		// _barracksIterator = 0;

	}

	public void Update(WorldState state)
	{
		// foreach (uint entityId in state.AddedEntities)
		// {
		// 	Entity entity = state.GetEntity(entityId)!;
		// 	if (entity.OwnerId != _playerId) continue;
		//
		// 	if (entity is Castle castle)
		// 	{
		// 		_castles.Add(castle);
		// 	}
		// 	else if (entity is Barracks barracks)
		// 	{
		// 		_barracks.Add(barracks);
		// 	}
		// }
	}

	// private Castle? GetNextCastle()
	// {
	// 	if (_castles.Count == 0) return null;
	//
	// 	while ((_castleIterator < _castles.Count))
	// 	{
	// 		Castle currCastle = _castles[_castleIterator];
	// 		if (currCastle.IsDestroyed)
	// 		{
	// 			_castleIterator++;
	// 			continue;
	// 		}
	// 		else if (currCastle.IsAtProductionCapacity)
	// 		{
	// 			_castlesClean.Add(currCastle);
	// 			_castleIterator++;
	// 			continue;
	// 		}
	//
	// 		_castleIterator++;
	// 	}
	//
	// 	if (_castleIterator >= _castles.Count)
	// 	{
	// 		List<Castle> tmp = _castles;
	// 		_castles = _castlesClean;
	// 		_castlesClean = tmp;
	//
	// 		_castlesClean.Clear();
	// 		_castleIterator = 0;
	//
	// 		return GetNextCastle();
	// 	}
	//
	// 	Castle curr = _castles[_castleIterator];
	// 	_castlesClean.Add(curr);
	// 	_castleIterator++;
	//
	// 	return curr;
	// }
	//
	// private Barracks? GetNextBarracks()
	// {
	// 	if (_barracks.Count == 0) return null;
	//
	// 	while ((_barracksIterator < _barracks.Count) && _barracks[_barracksIterator].IsDestroyed)
	// 	{
	// 		_barracksIterator++;
	// 	}
	//
	// 	if (_barracksIterator >= _barracks.Count)
	// 	{
	// 		List<Barracks> tmp = _barracks;
	// 		_barracks = _barracksClean;
	// 		_barracksClean = tmp;
	//
	// 		_barracksClean.Clear();
	// 		_barracksIterator = 0;
	//
	// 		return GetNextBarracks();
	// 	}
	//
	// 	Barracks curr = _barracks[_barracksIterator];
	// 	_barracksClean.Add(curr);
	// 	_barracksIterator++;
	//
	// 	return curr;
	// }

	public ICommand? ActionToCommand(WorldState state, RtsAction action)
	{
		switch (action)
		{
			case RtsAction.TrainWorker:
				return TrainWorker(state);
			case RtsAction.TrainKnight:
				return TrainKnight(state);
			case RtsAction.BuildBarracks:
				return BuildBarracks(state);
			case RtsAction.BuildCastle:
				return BuildCastle(state);
			case RtsAction.Attack:
				return Attack(state);
			case RtsAction.Defend:
				return Defend(state);
			case RtsAction.MineGold:
				return MineGold(state);
			case RtsAction.Wait:
				return Wait(state);
			default:
				return null;
		}
	}

	private ICommand? TrainWorker(WorldState state)
	{
		if (state.GetResource(_playerId, Resource.Gold) < BaseUnit.Dummy(UnitType.Worker).Cost.Amount) return null;

		IEnumerable<Castle> playerCastles = state.Structures.OfType<Castle>().Where(c => c.OwnerId != _playerId);

		Castle? minProductionCastle = null;
		int minProduction = int.MaxValue;

		foreach (Castle castle in playerCastles)
		{
			if (castle.OwnerId != _playerId) continue;
			if (castle.IsAtProductionCapacity) continue;

			int currProduction = castle.QueuedUnitsCount;

			if (currProduction < minProduction)
			{
				minProduction = currProduction;
				minProductionCastle = castle;
			}
		}
		
		if (minProductionCastle == null) return null;

		List<uint> entities = new List<uint> { minProductionCastle.Id };
		EnqueueUnitProductionCommandArgs args = new EnqueueUnitProductionCommandArgs(entities, UnitType.Worker);
		return new EnqueueUnitProductionCommand(_playerId, args);
	}

	private ICommand? TrainKnight(WorldState state)
	{
		if (state.GetResource(_playerId, Resource.Gold) < BaseUnit.Dummy(UnitType.Knight).Cost.Amount) return null;

		IEnumerable<Barracks> playerBarracks = state.Structures.OfType<Barracks>().Where(c => c.OwnerId != _playerId);

		Barracks? minProductionBarracks = null;
		int minProduction = int.MaxValue;

		foreach (Barracks barracks in playerBarracks)
		{
			if (barracks.OwnerId != _playerId) continue;
			if (barracks.IsAtProductionCapacity) continue;

			int currProduction = barracks.QueuedUnitsCount;

			if (currProduction < minProduction)
			{
				minProduction = currProduction;
				minProductionBarracks = barracks;
			}
		}
		
		if (minProductionBarracks == null) return null;

		List<uint> entities = new List<uint> { minProductionBarracks.Id };
		EnqueueUnitProductionCommandArgs args = new EnqueueUnitProductionCommandArgs(entities, UnitType.Knight);
		return new EnqueueUnitProductionCommand(_playerId, args);
	}

	private ICommand? BuildBarracks(WorldState state)
	{
		// when new building do 4 neighbours from it (left, right, up, down)
		// save the ones where buildings can be built
		// put them in a queue: the locations who appeared first are the ones to be used (should save the building and the direction)
		// dequeue the next position, if nothing can be placed there anymore, skip
		// if it can be placed, place it and add 4 neighbours.

		// what is "left"? we know the start, width and height of the building we are building left of, as well
		// as the building being built.
		// to the left specific, it would try to build on the x coordinate:
		// (curr.x - 1 - newBuilding.width)
		// for the "top" it's weirder since they have different lengths
		// go to (curr.y + curr.height + 1) and then try from (x = curr.x) to (x = curr.x + width - newBuilding.width)

		// put gold nodes in clumps instead of spread
		// make sure there are no "bridges" that are small enough that the AI could block movement when placing there
	}

	private ICommand? BuildCastle(WorldState state)
	{

	}

	private ICommand? Attack(WorldState state)
	{

	}

	private ICommand? Defend(WorldState state)
	{

	}

	private ICommand? MineGold(WorldState state)
	{

	}

	private ICommand? Wait(WorldState state)
	{
		return null;
	}

}

}
