using System;
using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Math;
using RtsEngine.Structures;

namespace RtsEngine.Commands
{

public class BuildNewCommand : EntityCommand<BuildNewCommandArgs>
{
	public BaseStructure? structure;

	public BuildNewCommand(uint playerId, BuildNewCommandArgs args) : base(playerId, args)
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

		structure = BaseStructure.FromType(_args.StructureType, state, PlayerId, _args.Start);

		return true;
	}

	protected override void ExecuteEntity(WorldState state, Entity entity)
	{
		((IBuilder)entity).Build(structure!);
	}
}

}
