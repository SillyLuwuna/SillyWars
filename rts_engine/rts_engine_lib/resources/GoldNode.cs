using RtsEngine.Data;
using RtsEngine.Math;

namespace RtsEngine.Resources
{

public class GoldNode : BaseResourceNode
{
	public override int MaxAmount { get => 100; }
	public override int RequiredWork { get => 100; }
	public override int MaxGatherers { get => 3; }
	public override int GatherAmount { get => 1; }
	public override Resource Resource { get => Resource.Gold; }

	public GoldNode(Vec2 pos, uint ownerId) : base(pos, ownerId, 0.5f)
	{
	}

	public override void SerializeFields(SerializerWriter writer) // ABSTRACT VARIABLES SERIALIZED BY IMPLEMENTATION
	{
		base.SerializeFields(writer);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader); // AFTER
	}
}

}
