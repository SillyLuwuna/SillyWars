using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.Math;

namespace RtsEngine.Commands
{

public class BuildCommandArgs : EntityCommandArgs
{
	public uint StructureId;

	public BuildCommandArgs(List<uint> entityIds, uint structureId) : base(entityIds)
	{
		StructureId = structureId;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
		writer.Write(StructureId);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
		reader.Read(out StructureId);
	}
}

}
