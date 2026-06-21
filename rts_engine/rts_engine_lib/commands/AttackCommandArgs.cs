using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.Math;

namespace RtsEngine.Commands
{

public class AttackCommandArgs : EntityCommandArgs
{
	public uint VictimId;

	public AttackCommandArgs(List<uint> entityIds, uint victimId) : base(entityIds)
	{
		VictimId = victimId;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
		writer.Write(VictimId);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
		reader.Read(out VictimId);
	}
}

}
