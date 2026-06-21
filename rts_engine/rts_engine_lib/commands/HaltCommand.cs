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

	protected override bool ValidateEntity(WorldState state, Entity? entity)
	{
		if (!base.ValidateEntity(state, entity)) return false;

		if (!(entity is IMovable)) return false;
		return true;
	}

	protected override void ExecuteEntity(WorldState state, Entity entity)
	{
		((IMovable)entity).Halt();
	}
}

}
