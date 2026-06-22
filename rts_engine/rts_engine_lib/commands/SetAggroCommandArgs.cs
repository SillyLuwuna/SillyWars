using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.Math;

namespace RtsEngine.Commands
{

public class SetAggroCommandArgs : EntityCommandArgs
{
	public bool Aggro;

	public SetAggroCommandArgs(List<uint> entityIds, bool aggro) : base(entityIds)
	{
		Aggro = aggro;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
		writer.Write(Aggro);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
		reader.Read(out Aggro);
	}
}

}
