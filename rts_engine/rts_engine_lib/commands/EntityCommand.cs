using RtsEngine.Data;
using RtsEngine.EntityProperties;

namespace RtsEngine.Commands
{

public abstract class EntityCommand<TArgs> : Command<TArgs> where TArgs : EntityCommandArgs
{
	protected Entity? EntityRef;

	public EntityCommand(uint playerId, TArgs args) : base(playerId, args)
	{
		EntityRef = null;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);

		bool isEntityNull = EntityRef == null;
		writer.Write<bool>(isEntityNull);

		if (isEntityNull) return;
		writer.Write(EntityRef!);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);

		bool isEntityNull = reader.Read<bool>();

		if (isEntityNull)
		{
			EntityRef = null;
			return;
		}

		EntityRef = reader.Read<Entity>();
	}

	public override bool Validate(WorldState state)
	{
		EntityRef = state.GetEntity(_args.EntityId);

		if (EntityRef == null) return false;
		if (EntityRef.OwnerId != PlayerId) return false;

		return true;
	}
}

}
