using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.EntityProperties;

namespace RtsEngine.Commands
{

public abstract class EntityCommand<TArgs> : Command<TArgs> where TArgs : EntityCommandArgs
{
	protected List<Entity> EntityRefs;

	public EntityCommand(uint playerId, TArgs args) : base(playerId, args)
	{
		EntityRefs = new List<Entity>();
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);

		// writer.Write(EntityRefs);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);

		EntityRefs = new List<Entity>();
		// reader.Read(out EntityRefs);
	}

	public override bool Validate(WorldState state)
	{
		foreach (uint entityId in _args.EntityIds)
		{
			Entity? entity = state.GetEntity(entityId);
			// if (!ValidateEntity(state, entity)) return false;
			if (!ValidateEntity(state, entity)) continue;
			EntityRefs.Add(entity!);
		}
		if (EntityRefs.Count == 0) return false;
		return true;
	}

	protected virtual bool ValidateEntity(WorldState state, Entity? entity)
	{
		if (entity == null) return false;
		if (entity.OwnerId != PlayerId) return false;
		return true;
	}

	protected override void ExecuteSpecific(WorldState state)
	{
		foreach (Entity entity in EntityRefs)
		{
			ExecuteEntity(state, entity);
		}
	}

	protected abstract void ExecuteEntity(WorldState state, Entity entity);
}

}
