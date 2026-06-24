using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.Math;

namespace RtsEngine.Commands
{

public class SetProductionSpawnpointCommandArgs : EntityCommandArgs
{
	public Vec2 Spawnpoint;

	public SetProductionSpawnpointCommandArgs(List<uint> entityIds, Vec2 spawnpoint) : base(entityIds)
	{
		Spawnpoint = spawnpoint;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
		writer.Write(Spawnpoint);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
		Spawnpoint = reader.Read<Vec2>();
	}
}

}
