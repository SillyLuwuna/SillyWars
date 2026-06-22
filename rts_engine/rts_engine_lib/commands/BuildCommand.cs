using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Math;
using RtsEngine.Structures;

namespace RtsEngine.Commands
{

public class BuildCommand : EntityCommand<BuildCommandArgs>
{
	public Entity? structure;

	public BuildCommand(uint playerId, BuildCommandArgs args) : base(playerId, args)
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

		if (!(entity is IBuilder)) return false;

		structure = state.GetEntity(_args.StructureId);
		if (structure == null) return false;
		if (structure.OwnerId != PlayerId) return false;
		if (!(structure is BaseStructure)) return false;

		return true;
	}

	protected override void ExecuteEntity(WorldState state, Entity entity)
	{
		((IBuilder)entity).Build((BaseStructure)structure!);
	}
}

}
