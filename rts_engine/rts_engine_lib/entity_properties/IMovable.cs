using RtsEngine.Map;
using RtsEngine.Math;
using RtsEngine.Units;

namespace RtsEngine.EntityProperties
{

public interface IMovable : IPositionable, IStateful
{
	public float MoveSpeed { get; set; }

	public void SetGoal(Grid<Cell> map, Vec2 goal);

	public void Halt();
}

}

