namespace RtsEngine;

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
		// string typeName = reader.ReadString();
		// Type type = Type.GetType(typeName)
		// 	?? throw new InvalidDataException("Null type entity.");
		//
		// Entity instance = (Entity)(Activator.CreateInstance(type, true) ?? throw new InvalidDataException("Null type entity."));
		//
		// instance.DeserializeInternal(reader);
		// return instance;
	}

	// protected virtual void DeserializeInternal(BinaryReader reader)
	// {
	// 	Id = reader.ReadUInt32();
	// 	if (_CURR_ID <= Id)
	// 	{
	// 		_CURR_ID = Id + 1;
	// 	}
	// 	Pos.Deserialize(reader);
	// }

	// public static Entity Deserialize(BinaryReader reader)
	// {
	// 	string typeName = reader.ReadString();
	// 	Type? type = Type.GetType(typeName);
	// 	if (type == null)
	// 	{
	// 		throw new InvalidDataException("Null type entity.");
	// 	}
	//
	// 	var deserializeMethod = type.GetMethod("Deserialize");
	//
	// 	// Entity entity = Type.Deserialize(reader);
	//
	// 	Entity entity = DeserializeEntity(reader);
	// 	Entity e = new base();
	// 	Id = reader.ReadUInt32();
	// 	if (_CURR_ID <= Id)
	// 	{
	// 		_CURR_ID = Id + 1;
	// 	}
	// 	Pos.Deserialize(reader);
	// }

	// {
	// 	Entity entity = DeserializeEntity(reader);
	// 	Entity e = new base();
	// 	Id = reader.ReadUInt32();
	// 	if (_CURR_ID <= Id)
	// 	{
	// 		_CURR_ID = Id + 1;
	// 	}
	// 	Pos.Deserialize(reader);
	// }

	public abstract void Tick();
}
