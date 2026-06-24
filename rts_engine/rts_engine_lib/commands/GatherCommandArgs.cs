using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.EntityProperties;

namespace RtsEngine.Commands
{

public class GatherCommandArgs : EntityCommandArgs
{
	public uint GatherableId;

	public GatherCommandArgs(List<uint> entityIds, IGatherable gatherable) : base(entityIds)
	{
		GatherableId = gatherable.Id;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
		writer.Write(GatherableId);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
		GatherableId = reader.Read<uint>();
	}
}

}
