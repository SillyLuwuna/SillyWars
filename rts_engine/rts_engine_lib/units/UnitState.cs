using System.IO;
using RtsEngine.Data;

namespace RtsEngine.Units
{

public struct UnitState : ISerializable
{
	private byte _stateFlags;
	public const byte WALK_MASK = (1 << 0);
	public const byte AGGRO_MASK = (1 << 1);
	public const byte GOAL_MASK = (1 << 2) | (1 << 3);

	// 1 bit - idle/walking (movement)
	// 2 bit - neutral/aggro (aggressiveness)
	// 3-4 bit - none/build/mine/retrieve (goal)

	public bool IsWalking
	{
		get => (_stateFlags & WALK_MASK) != 0;
		set
		{
			if (value)
				_stateFlags |= WALK_MASK;
			else
				_stateFlags &= unchecked((byte)~WALK_MASK);
		}
	}

	public bool IsIdle
	{
		get => !IsWalking;
		set => IsWalking = !value;
	}

	public bool IsAggro
	{
		get => (_stateFlags & AGGRO_MASK) != 0;
		set
		{
			if (value)
				_stateFlags |= AGGRO_MASK;
			else
				_stateFlags &= unchecked((byte)~AGGRO_MASK);
		}
	}

	public bool IsNeutral
	{
		get => !IsAggro;
		set => IsAggro = !value;
	}

	public UnitGoal Goal
	{
		get => (UnitGoal)((_stateFlags & GOAL_MASK) >> 2);
		set
		{
			_stateFlags &= unchecked((byte)~GOAL_MASK);
			_stateFlags |= (byte)((int)value << 2);
		}
	}

	public void SerializeFields(BinaryWriter writer)
	{
		writer.Write(_stateFlags);
	}

	public void DeserializeFields(BinaryReader reader)
	{
		_stateFlags = reader.ReadByte();
	}
}
}
