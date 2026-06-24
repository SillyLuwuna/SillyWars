using System;
using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Math;
using RtsEngine.Physics;

namespace RtsEngine.Resources
{

public abstract class BaseResourceNode: PhysicsObject, ISerializable, IGatherable
{
	public abstract int MaxAmount { get; }
	public abstract int RequiredWork { get; }
	public abstract int MaxGatherers { get; }
	public abstract int GatherAmount { get; }
	public abstract Resource Resource { get; }

	// should be ordered in decreaseing order
	public int CurrGathererCount { get; private set; }
	private Dictionary<IGatherer, int> _gatherersWorkDone = null!;
	private HashSet<IGatherer> _activeGatherers = null!;

	public int Remaining { get; protected set; }

	public BaseResourceNode(Vec2 pos, uint ownerId, float radius) : base(pos, ownerId, 1.0f, radius, 1.0f, true)
	{
		Init();

		Remaining = MaxAmount;
	}

	private void Init()
	{
		CurrGathererCount = 0;
		_gatherersWorkDone = new Dictionary<IGatherer, int>();
		_activeGatherers = new HashSet<IGatherer>();
	}

	public override void SerializeFields(SerializerWriter writer) // ABSTRACT VARIABLES SERIALIZED BY IMPLEMENTATION
	{
		base.SerializeFields(writer);

		writer.Write(CurrGathererCount);
		writer.Write(Remaining);
		// doesn't save active gatherers nor the gatherer's work done
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);

		Init();

		CurrGathererCount = reader.Read<int>();
		Remaining = reader.Read<int>();
	}

	public bool IsDepleted { get => Remaining <= 0; }

	public bool IsInGatheringRange(IGatherer gatherer)
	{
		return (gatherer.Pos.Distance(this.Pos) - this.Radius) <= gatherer.GatherRange;
	}

	// returns a negative value if the gatherer cannot gather
	public ResourceStack TryGather(IGatherer gatherer)
	{
		if (IsDepleted) return new ResourceStack(Resource.None, -1);

		if (!IsInGatheringRange(gatherer)) return new ResourceStack(Resource.None, -1);

		if (!_gatherersWorkDone.ContainsKey(gatherer))
		{
			if (_gatherersWorkDone.Count >= MaxGatherers) return new ResourceStack(Resource.None, -1);
			_gatherersWorkDone[gatherer] = RequiredWork;
			_activeGatherers.Add(gatherer);
			return new ResourceStack(this.Resource, 0);
		}

		_gatherersWorkDone[gatherer] -= gatherer.WorkPerGather;
		_activeGatherers.Add(gatherer);
		if (_gatherersWorkDone[gatherer] > 0) return new ResourceStack(this.Resource, 0);

		_gatherersWorkDone[gatherer] = RequiredWork;
		_gatherersWorkDone.Remove(gatherer);
		_activeGatherers.Remove(gatherer);

		Remaining -= GatherAmount;

		if (IsDepleted)
		{
			HandleDepletion();
		}

		return new ResourceStack(this.Resource, GatherAmount);
	}

	private void HandleDepletion()
	{
		RtsEngine.Instance.State.RemoveEntity(this);
	}

	public override void Tick()
	{
		base.Tick();

		ClearInactiveGatherers();
	}

	private void ClearInactiveGatherers()
	{
		foreach (IGatherer gatherer in _gatherersWorkDone.Keys)
		{
			if (!_activeGatherers.Contains(gatherer))
			{
				_gatherersWorkDone.Remove(gatherer);
			}
		}

		_activeGatherers.Clear();
	}
}

}
