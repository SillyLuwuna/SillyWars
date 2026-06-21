using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.Math;

namespace RtsEngine.Commands
{

public class MoveCommandArgs : EntityCommandArgs
{
	public Vec2 Goal;

	public MoveCommandArgs(List<uint> entityIds, Vec2 goal) : base(entityIds)
	{
		Goal = goal;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
		writer.Write(Goal);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
		reader.Read(out Goal);
	}
}

}
