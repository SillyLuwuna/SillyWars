using System;
using System.Collections.Generic;
using System.IO;
using RtsEngine.Data;
using RtsEngine.Math;

namespace RtsEngine.EntityProperties
{

public abstract class Entity : ITickable, ISerializable, IEquatable<Entity>, IPositionable
{
	private static uint _CURR_ID = 0;

	public uint Id;
	public uint OwnerId;
	protected Vec2 _pos;
	public Vec2 Pos { get => _pos; set => _pos = value; }

	public Entity(Vec2 pos, uint ownerId)
	{
		Id = _CURR_ID;
		_CURR_ID++;
		OwnerId = ownerId;
		Pos = pos;
	}

	public virtual void SerializeFields(SerializerWriter writer)
	{
		writer.Write(Id);
		writer.Write(OwnerId);
		writer.Write(Pos);
	}

	public virtual void DeserializeFields(SerializerReader reader)
	{
		Id = reader.Read<uint>();
		OwnerId = reader.Read<uint>();
		Pos = reader.Read<Vec2>();

		if (_CURR_ID <= Id)
		{
			_CURR_ID = Id + 1;
		}
	}

	public abstract void Tick();

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType()) return false;

		return Equals((Entity)obj);
	}

	public bool Equals(Entity other)
	{
		if (other == null) return false;

		return (this.Id == other.Id);
	}

	public override int GetHashCode()
	{
		return (int)Id;
	}
}

}
