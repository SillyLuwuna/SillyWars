using System;
using RtsEngine.Data;

namespace RtsEngine.EntityProperties
{

public interface IEntity : ITickable, ISerializable, IEquatable<Entity>
{
	public uint Id { get; }
	public uint OwnerId { get; set; }
}

}

