using System;
using System.IO;
using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using RtsEngine.Math;

namespace RtsEngine.Physics
{

public abstract class PhysicsObject : Entity
{
	private static float COLLISION_PUSHBACK_INTENSITY = 0.20f;
	private static float COLLISION_PUSHBACK_DAMPENING = 0.10f;

	public Vec2 Force { get; private set; }
	public Vec2 Velocity { get; private set; }

	public float Radius { get; protected set; }
	public float Friction { get; protected set; }
	public float Mass { get; protected set; }

	public Vec2 DeltaPos { get; private set; }

	private bool _enabled;
	public bool Enabled
	{
		get => _enabled;
		set 
		{
			ClearForces();
			_enabled = value;
		}
	}

	public PhysicsObject(Vec2 pos, uint ownerId, float mass, float radius, float friction) : base(pos, ownerId)
	{
		Radius = radius;
		Friction = friction;
		Mass = mass;
		_enabled = true;
	}

	public void ClearForces()
	{
		Force = new Vec2(0, 0);
	}

	public void ClearVelocity()
	{
		Velocity = new Vec2(0, 0);
	}

	public void ApplyForce(Vec2 force)
	{
		Force += force;
	}

	public bool Collides(PhysicsObject other)
	{
		return this.Pos.Distance(other.Pos) <= (this.Radius + other.Radius);
	}

	public void ProcessCollision(PhysicsObject other)
	{
		if (!this.Collides(other)) return;

		float distance = this.Pos.Distance(other.Pos);

		Vec2 direction = this.Pos.To(other.Pos);
		if (direction.IsZero)
		{
			direction = new Vec2(0f, 1.0f);
		}

		direction = direction.Unit;

		float magnitude = COLLISION_PUSHBACK_DAMPENING / (COLLISION_PUSHBACK_DAMPENING + distance);
		magnitude *= COLLISION_PUSHBACK_INTENSITY;

		Vec2 pushbackForce = direction * magnitude;
		this.ApplyForce(-pushbackForce);
		other.ApplyForce(pushbackForce);
	}

	public void PhysicsTick()
	{
		if (!Enabled) return;

		Vec2 oldPos = new Vec2(Pos.x, Pos.y);

		Vec2 acceleration = Force / Mass;
		Velocity += acceleration;
		Pos += Velocity;

		Velocity *= (1 - Friction);

		ClearForces();

		DeltaPos = Pos - oldPos;
	}


	// WARNING only works for Radius < map._stepSize / 2
	// public void LimitToBoundaries(Grid<Cell> map)
	public void LimitToBoundaries(Grid<Cell> map)
	{
		Vec2 oldPos = Pos - DeltaPos;

		if (!map.ContainsPosFromWorldSpace(oldPos)) return;
		if (!map.GetObjectAtWorldPos(oldPos).IsWalkable) return;

		Vec2Int oldCellPos = map.CellPosFromWorldSpace(oldPos);

		Vec2 dx = new Vec2(oldPos.x + DeltaPos.x, oldPos.y);
		Vec2 dy = new Vec2(oldPos.x, oldPos.y + DeltaPos.y);

		float error = 0.0001f;

		if (!map.ContainsPosFromWorldSpace(dx) || !map.GetObjectAtWorldPos(dx).IsWalkable)
		{
			if (DeltaPos.x > 0)
			{
				Pos.x = map.RightEdgeX(oldCellPos) - error;
			}
			else
			{
				Pos.x = map.LeftEdgeX(oldCellPos) + error;
			}
		}

		if (!map.ContainsPosFromWorldSpace(dy) || !map.GetObjectAtWorldPos(dy).IsWalkable)
		{
			if (DeltaPos.y > 0)
			{
				Pos.y = map.UpEdgeY(oldCellPos) - error;
			}
			else
			{
				Pos.y = map.DownEdgeY(oldCellPos) + error;
			}
		}
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);

		writer.Write(Force);
		writer.Write(Velocity);
		writer.Write(DeltaPos);

		writer.Write(Radius);
		writer.Write(Friction);
		writer.Write(Mass);

		writer.Write(_enabled);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);

		Force = reader.Read<Vec2>();
		Velocity = reader.Read<Vec2>();
		DeltaPos = reader.Read<Vec2>();

		Radius = reader.Read<float>();
		Friction = reader.Read<float>();
		Mass = reader.Read<float>();

		_enabled = reader.Read<bool>();
	}
}

}

