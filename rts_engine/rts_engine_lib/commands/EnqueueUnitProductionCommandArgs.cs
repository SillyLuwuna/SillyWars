using System.Collections.Generic;
using RtsEngine.Data;
using RtsEngine.Math;
using RtsEngine.Units;

namespace RtsEngine.Commands
{

public class EnqueueUnitProductionCommandArgs : EntityCommandArgs
{
	public UnitType ProductionUnitType;

	public EnqueueUnitProductionCommandArgs(List<uint> entityIds, UnitType unitType) : base(entityIds)
	{
		ProductionUnitType = unitType;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
		writer.Write(ProductionUnitType);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);
		ProductionUnitType = reader.Read<UnitType>();
	}
}

}
