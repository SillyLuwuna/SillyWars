using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Math;

namespace RtsEngine.Commands
{

public class AttackCommand : EntityCommand<AttackCommandArgs>
{
	public Entity? victim;

	public AttackCommand(uint playerId, AttackCommandArgs args) : base(playerId, args)
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

		if (!(entity is IAttacker)) return false;

		victim = state.GetEntity(_args.VictimId);
		if (victim == null) return false;
		if (victim.OwnerId == PlayerId) return false;
		if (!(victim is IDestroyable)) return false;
		return true;
	}

	protected override void ExecuteEntity(WorldState state, Entity entity)
	{
		((IAttacker)entity).Attack((IDestroyable)entity);
	}
}

}
