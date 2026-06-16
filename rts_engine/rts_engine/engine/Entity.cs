namespace RtsEngine
{

public abstract class Entity : ITickable, ISerializable
{
	private static uint _CURR_ID = 0;

	public uint Id;
	public Vec2 Pos;

	public Entity()
	{
		Id = _CURR_ID;
		_CURR_ID++;
	}

	public virtual void SerializeFields(BinaryWriter writer)
	{
		writer.Write(Id);
		Pos.SerializeFields(writer);
	}

	public virtual void DeserializeFields(BinaryReader reader)
	{
		Id = reader.ReadUInt32();
		if (_CURR_ID <= Id)
		{
			_CURR_ID = Id + 1;
		}
		Pos.DeserializeFields(reader);
	}

	public abstract void Tick();
}

}
