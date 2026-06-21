using RtsEngine.Data;

namespace RtsEngine.Commands
{

public abstract class Command<TArgs> : ICommand where TArgs : CommandArgs
{
	public uint PlayerId { get; set; }
	protected TArgs _args;

	public Command(uint playerId, TArgs args)
	{
		PlayerId = playerId;
		_args = args;
	}

	public virtual void SerializeFields(SerializerWriter writer)
	{
		writer.Write(PlayerId);
		writer.Write(_args);
	}

	public virtual void DeserializeFields(SerializerReader reader)
	{
		PlayerId = reader.Read<uint>();
		_args = reader.Read<TArgs>();
	}

	public abstract bool Validate(WorldState state);

	public void Execute(WorldState state)
	{
		if (!Validate(state)) return;

		ExecuteSpecific(state);
	}

	protected abstract void ExecuteSpecific(WorldState state);
}

}
