using RtsEngine.Map;
using RtsEngine.Math;

namespace RtsEngine.EntityProperties
{

public interface IMover
{
	public void Move(Grid<Cell> map, Vec2 pos);
	public void Halt();
}

}

