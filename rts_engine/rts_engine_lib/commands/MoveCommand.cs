using RtsEngine.Data;
using RtsEngine.EntityProperties;

namespace RtsEngine.Commands
{

public class MoveCommand : EntityCommand<MoveCommandArgs>
{
	public MoveCommand(uint playerId, MoveCommandArgs args) : base(playerId, args)
	{
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
	}

	public override bool Validate(WorldState state)
	{
		if (!base.Validate(state)) return false;
		if (!(EntityRef is IMovable)) return false;
		return true;
	}

	public override void ExecuteSpecific(WorldState state)
	{
		MoveCommandArgs args = (MoveCommandArgs)_args;

		((IMovable)EntityRef!).Move(state.Map, args.Goal);
	}
}

}
