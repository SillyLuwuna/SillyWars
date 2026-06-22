using System;
using RtsEngine.Data;

namespace RtsEngine.Units
{

public class StateEventArgs : EventArgs
{
	public State OldState;
	public State NewState;

	public StateEventArgs(State oldState, State newState) : base()
	{
		OldState = oldState;
		NewState = newState;
	}
}

public class State : ISerializable
{
	private byte _stateFlags;

	public event EventHandler<StateEventArgs>? Changed;

	public const byte WALK_MASK = (1 << 0);
	public const byte AGGRO_MASK = (1 << 1);
	public const byte GOAL_MASK = (1 << 2) | (1 << 3) | (1 << 4);

	// 1 bit - idle/walking (movement)
	// 2 bit - neutral/aggro (aggressiveness)
	// 3-5 bit - none/build/mine/retrieve/attack/walk/?/? (goal)

	public State()
	{
		_stateFlags = 0;
	}

	private State(byte stateFlags)
	{
		_stateFlags = stateFlags;
	}

	public bool IsWalking
	{
		get => (_stateFlags & WALK_MASK) != 0;
		set
		{
			byte oldFlags = _stateFlags;

			if (value)
				_stateFlags |= WALK_MASK;
			else
				_stateFlags &= unchecked((byte)~WALK_MASK);

			OnStateChange(oldFlags, _stateFlags);
		}
	}

	public bool IsAggro
	{
		get => (_stateFlags & AGGRO_MASK) != 0;
		set
		{
			byte oldFlags = _stateFlags;

			if (value)
				_stateFlags |= AGGRO_MASK;
			else
				_stateFlags &= unchecked((byte)~AGGRO_MASK);

			OnStateChange(oldFlags, _stateFlags);
		}
	}

	public Goal Goal
	{
		get => (Goal)((_stateFlags & GOAL_MASK) >> 2);
		set
		{
			byte oldFlags = _stateFlags;

			_stateFlags = (byte)((_stateFlags & ~GOAL_MASK) | ((int)value << 2));

			OnStateChange(oldFlags, _stateFlags);

			// _stateFlags &= unchecked((byte)~GOAL_MASK);
			// _stateFlags |= (byte)((int)value << 2);
		}
	}

	public void SerializeFields(SerializerWriter writer)
	{
		writer.Write(_stateFlags);
	}

	public void DeserializeFields(SerializerReader reader)
	{
		reader.Read(out _stateFlags);
	}

	public void OnStateChange(byte oldStateFlags, byte newStateFlags)
	{
		StateEventArgs args = new StateEventArgs(new State(oldStateFlags), new State(newStateFlags));
		Changed?.Invoke(this, args);
	}
}
}
