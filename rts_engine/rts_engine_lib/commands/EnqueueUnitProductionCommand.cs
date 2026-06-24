using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Math;
using RtsEngine.Structures;

namespace RtsEngine.Commands
{

public class EnqueueUnitProductionCommand : EntityCommand<EnqueueUnitProductionCommandArgs>
{
	public EnqueueUnitProductionCommand(uint playerId, EnqueueUnitProductionCommandArgs args) : base(playerId, args)
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

		if (!(entity is UnitProducer)) return false;

		return true;
	}

	protected override void ExecuteEntity(WorldState state, Entity entity)
	{
		((UnitProducer)entity).EnqueueProduction(_args.ProductionUnitType);
	}
}

}
