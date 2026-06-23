using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.Math;
using RtsEngine.Structures;

namespace RtsEngine.Commands
{

public class BuildNewCommandArgs : EntityCommandArgs
{
	public Vec2Int Start;
	public StructureType StructureType;

	public BuildNewCommandArgs(List<uint> entityIds, Vec2Int start, StructureType structureType) : base(entityIds)
	{
		Start = start;
		StructureType = structureType;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
		writer.Write(Start);
		writer.Write(StructureType);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
		Start = reader.Read<Vec2Int>();
		StructureType = reader.Read<StructureType>();
	}
}

}
