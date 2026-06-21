using System.Collections.Generic;
using RtsEngine.Data;

namespace RtsEngine.Commands
{

public class EntityCommandArgs : CommandArgs
{
	public List<uint> EntityIds;

	public EntityCommandArgs(List<uint> entityIds) : base()
	{
		EntityIds = entityIds;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		writer.Write(EntityIds);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		reader.Read(out EntityIds);
	}
}

}
