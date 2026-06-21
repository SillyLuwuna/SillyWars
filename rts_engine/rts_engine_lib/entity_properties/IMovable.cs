using RtsEngine.Map;
using RtsEngine.Math;

namespace RtsEngine.EntityProperties
{

public interface IMovable
{
	public void Move(Grid<Cell> map, Vec2 pos);
	public void Halt();
	public float GetRadius();
}

}

