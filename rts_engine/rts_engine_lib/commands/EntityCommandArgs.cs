using RtsEngine.Data;

namespace RtsEngine.Commands
{

public class EntityCommandArgs : CommandArgs
{
	public uint EntityId;

	public EntityCommandArgs(uint entityId) : base()
	{
		EntityId = entityId;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		writer.Write(EntityId);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		reader.Read(out EntityId);
	}
}

}
