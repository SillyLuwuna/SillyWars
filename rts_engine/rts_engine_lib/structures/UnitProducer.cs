using System;
using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using RtsEngine.Math;
using RtsEngine.Units;

namespace RtsEngine.Structures
{

public abstract class UnitProducer : BaseStructure
{
	public abstract int MaxUnitProduction { get; set; }
	public abstract List<UnitType> AllowedUnitTypes { get; set; }

	public Vec2? SpawnPosition { get; private set; }
	public Vec2? SpawnTarget { get; set; }

	private Queue<UnitType> _productionQueue = null!;
	private UnitType? _productionUnit;

	private int _productionCooldown;


	public UnitProducer(uint ownerId, Vec2Int start, int height, int width) : base(ownerId, start, height, width)
	{
		Init();
	}

	private void Init()
	{
		_productionQueue = new Queue<UnitType>();
		_productionUnit = null;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);

		writer.Write(MaxUnitProduction);
		writer.Write(AllowedUnitTypes);

		writer.Write(_productionQueue.Count);
		foreach (UnitType unitType in _productionQueue)
		{
			writer.Write(unitType);
		}

		writer.Write(_productionUnit);
		writer.Write(_productionCooldown);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);

		Init();

		MaxUnitProduction = reader.Read<int>();
		AllowedUnitTypes = reader.Read<List<UnitType>>();

		int numUnitsOnProductionQueue = reader.Read<int>();
		for (int i = 0; i < numUnitsOnProductionQueue; i++)
		{
			_productionQueue.Enqueue(reader.Read<UnitType>());
		}

		_productionUnit = reader.Read<UnitType?>();
		_productionCooldown = reader.Read<int>();
	}

	public bool CanProduce(UnitType unitType)
	{
		foreach (UnitType allowedType in AllowedUnitTypes)
		{
			if (allowedType == unitType) return true;
		}

		return false;
	}

	public void EnqueueProduction(UnitType unitType)
	{
		if (!CanProduce(unitType)) return;
		if (QueuedUnitsCount >= MaxUnitProduction) return;

		WorldState state = RtsEngine.Instance.State;

		if (state.HasMaximumUnits(this.OwnerId)) return;
		int currExpectedUnits = state.PlayerUnitsCount(this.OwnerId) + state._playerTotalEnqueuedUnits[this.OwnerId];
		if (currExpectedUnits >= state.MaxPlayerUnits) return;
		if (!state.TryTakeResource(BaseUnit.Dummy(unitType).Cost, this.OwnerId)) return;

		RtsEngine.Instance.State._playerTotalEnqueuedUnits[this.OwnerId]++;
		_productionQueue.Enqueue(unitType);
	}

	public void ClearProduction()
	{
		_productionUnit = null;
		_productionQueue.Clear();
	}

	public override void Tick()
	{
		DecreaseCooldowns();

		if (!IsProducingUnits && _productionQueue.Count > 0)
		{
			DequeueNextUnit();
			return;
		}

		if (!IsProducingUnits) return;
		if (_productionCooldown > 0) return;

		UpdateSpawnLocation(); // inefficient, should be event based with map updates
		ProduceUnit();
	}

	private void DecreaseCooldowns()
	{
		if (_productionCooldown > 0)
		{
			_productionCooldown--;
		}
	}

	private void UpdateSpawnLocation()
	{
		Grid<Cell> map = RtsEngine.Instance.State.Map;
		foreach (Vec2Int tile in this.SurroundingTiles)
		{
			if (map.ContainsPos(tile) && map[tile].IsWalkable)
			{
				SpawnPosition = map.WorldSpaceFromCellPos(tile);
				return;
			}
		}

		SpawnPosition = null;
	}

	private void ProduceUnit()
	{
		BaseUnit unit = BaseUnit.FromUnitType(_productionUnit!.Value, this.OwnerId, SpawnPosition!.Value);
		_productionUnit = null;

		if (SpawnTarget != null)
		{
			unit.SetGoal(SpawnTarget.Value);
		}

		RtsEngine.Instance.State._playerTotalEnqueuedUnits[this.OwnerId]--;
		RtsEngine.Instance.State.AddEntity(unit);
	}

	private void DequeueNextUnit()
	{
		_productionUnit = _productionQueue.Dequeue();
		_productionCooldown = BaseUnit.FromUnitType(_productionUnit.Value, this.OwnerId, Vec2.Zero).ProductionTime; // inefficient
	}

	public bool IsProducingUnits { get => _productionUnit != null; }

	public int QueuedUnitsCount { get => IsProducingUnits ? _productionQueue.Count + 1 : _productionQueue.Count; }

	public UnitType? ProductionQueueHead { get => _productionUnit; }

	public Queue<UnitType> ProductionQueue { get => new Queue<UnitType>(_productionQueue); }

	public int TicksLeftForProduction { get => _productionCooldown; }
}

}
