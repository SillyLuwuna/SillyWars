using RtsEngine.Math;

namespace RtsEngine.EntityProperties
{

public interface IMovable : IPositionable
{
	public float MoveSpeed { get; set; }

	public void SetGoal(Vec2 goal);

	public void Halt();
}

}

