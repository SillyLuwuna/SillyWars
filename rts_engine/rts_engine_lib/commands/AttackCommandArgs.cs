using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.Math;

namespace RtsEngine.Commands
{

public class AttackCommandArgs : EntityCommandArgs
{
	public uint TargetId;

	public AttackCommandArgs(List<uint> entityIds, uint targetId) : base(entityIds)
	{
		TargetId = targetId;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
		writer.Write(TargetId);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
		reader.Read(out TargetId);
	}
}

}
