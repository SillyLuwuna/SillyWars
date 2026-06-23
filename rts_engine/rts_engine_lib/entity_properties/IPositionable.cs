using RtsEngine.Math;

namespace RtsEngine.EntityProperties
{

public interface IPositionable : IEntity
{
	public Vec2 Pos { get; set; }
}

}

