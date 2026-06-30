using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Math;

namespace RtsEngine.Commands
{

public class AggroMoveCommand : EntityCommand<AggroMoveCommandArgs>
{
	public AggroMoveCommand(uint playerId, AggroMoveCommandArgs args) : base(playerId, args)
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
		if (!(entity is IAttacker)) return false;

		return true;
	}

	protected override void ExecuteEntity(WorldState state, Entity entity)
	{
		((IMovable)entity).SetGoal(_args.Goal);
		((IAttacker)entity).SetAggro(_args.Aggro);
	}
}

}
