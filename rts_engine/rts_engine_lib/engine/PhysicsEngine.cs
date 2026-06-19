using System.Collections.Generic;
using RtsEngine.EntityProperties;
using RtsEngine.Map;

namespace RtsEngine
{

public class PhysicsEngine
{
	public PhysicsEngine()
	{

	}

	public void ProcessCollisions(List<PhysicsObject> physicsObjects)
	{
		// List<PhysicsObject> physicsObjects = state.GetPhysicsObjects();
		for (int i = 0; i < physicsObjects.Count; i++)
		{
			for (int j = i + 1; j < physicsObjects.Count; j++)
			{
				physicsObjects[i].ProcessCollision(physicsObjects[j]);
			}
		}
	}

	public void PhysicsTick(List<PhysicsObject> physicsObjects)
	{
		foreach (PhysicsObject phyObj in physicsObjects)
		{
			phyObj.PhysicsTick();
		}
	}

	public void LimitToMapBoundaries(List<PhysicsObject> physicsObjects, Grid<Cell> map)
	{
		foreach (PhysicsObject phyObj in physicsObjects)
		{
			phyObj.LimitToBoundaries(map);
		}
	}
}

}
