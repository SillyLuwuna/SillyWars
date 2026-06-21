using RtsEngine.Data;
using RtsEngine.EntityProperties;

namespace RtsEngine.Commands
{

public class HaltCommand : EntityCommand<EntityCommandArgs>
{
	public HaltCommand(uint playerId, EntityCommandArgs args) : base(playerId, args)
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
		if (!(EntityRef is IMovable correctEntity)) return false;
		return true;
	}

	public override void ExecuteSpecific(WorldState state)
	{
		((IMovable)EntityRef!).Halt();
	}
}

}
