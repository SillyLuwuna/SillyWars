namespace RtsEngine;

public abstract class Entity : ITickable
{
	public Vec2 Pos;

	public abstract void Tick();
}
