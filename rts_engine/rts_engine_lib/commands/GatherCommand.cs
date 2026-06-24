using RtsEngine.Data;
using RtsEngine.EntityProperties;

namespace RtsEngine.Commands
{

public class GatherCommand : EntityCommand<GatherCommandArgs>
{
	public IGatherable? Gatherable;

	public GatherCommand(uint playerId, GatherCommandArgs args) : base(playerId, args)
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

		if (!(entity is IGatherer)) return false;
		
		Entity? gatherableEntity = state.GetEntity(_args.GatherableId);
		if (gatherableEntity == null) return false;
		if (!(gatherableEntity is IGatherable gatherable)) return false;

		Gatherable = gatherable;

		return true;
	}

	protected override void ExecuteEntity(WorldState state, Entity entity)
	{
		((IGatherer)entity).Gather(Gatherable!);
	}
}

}
