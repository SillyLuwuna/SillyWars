using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.Math;

namespace RtsEngine.Commands
{

public class AggroMoveCommandArgs : EntityCommandArgs
{
	public Vec2 Goal;
	public bool Aggro;

	public AggroMoveCommandArgs(List<uint> entityIds, Vec2 goal, bool aggro) : base(entityIds)
	{
		Goal = goal;
		Aggro = aggro;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
		writer.Write(Goal);
		writer.Write(Aggro);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
		reader.Read(out Goal);
		reader.Read(out Aggro);
	}
}

}
