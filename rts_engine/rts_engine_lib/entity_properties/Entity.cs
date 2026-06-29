using RtsEngine.Data;

namespace RtsEngine.EntityProperties
{

public abstract class Entity : IEntity
{
	private static uint _CURR_ID = 0;

	public WorldState World { get; set; }
	public uint Id { get; private set; }
	public uint OwnerId { get; set; }

	public Entity(uint ownerId, WorldState world)
	{
		Id = _CURR_ID;
		_CURR_ID++;
		OwnerId = ownerId;
		World = world;
	}

	public virtual void SerializeFields(SerializerWriter writer)
	{
		writer.Write(Id);
		writer.Write(OwnerId);
	}

	public virtual void DeserializeFields(SerializerReader reader)
	{
		Id = reader.Read<uint>();
		OwnerId = reader.Read<uint>();

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
		return (this.Id == other.Id);
	}

	public static bool operator ==(Entity? left, Entity? right)
	{
		if (left is null && right is null) return true;
		if (left is null || right is null) return false;

		return left.Equals(right);
	}

	public static bool operator !=(Entity? left, Entity? right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return (int)Id;
	}
}

}
