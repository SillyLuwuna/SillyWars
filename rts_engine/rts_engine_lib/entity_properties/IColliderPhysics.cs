using RtsEngine.Map;
using RtsEngine.Math;

namespace RtsEngine.EntityProperties
{

public interface IColliderPhysics
{
	public void ClearForces();
	public void AddForce(Vec2 force);
	public Vec2 GetForce();
	public float Radius();
}

}

