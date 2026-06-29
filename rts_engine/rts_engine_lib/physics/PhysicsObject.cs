using System;
using System.IO;
using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Map;
using RtsEngine.Math;

namespace RtsEngine.Physics
{

public class PhysicsObject : Entity, IPositionable
{
	private const float CollisionPushbackIntensity = 20.00f;
	private const float CollisionPushbackDampening = 0.001f;
	private const float CollisionPushbackClamp = 0.20f;
	private const float CollisionPushbackMin = 0.10f;
	private const float CollisionPushbackTotalClamp = 1.00f;

	public Vec2 Pos { get; set; }

	public Vec2 Force { get; private set; }
	public Vec2 Velocity { get; private set; }

	public float Radius { get; protected set; }
	public float Friction { get; protected set; }
	public float Mass { get; protected set; }

	public Vec2 DeltaPos { get; private set; }

	private Vec2 _totalPushbackForce;

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

	public bool IsStatic { get; set; }

	public PhysicsObject(Vec2 pos, WorldState world, uint ownerId, float mass, float radius, float friction, bool isStatic = false) : base(ownerId, world)
	{
		Pos = pos;
		_totalPushbackForce = Vec2.Zero;
		Radius = radius;
		Friction = friction;
		Mass = mass;
		_enabled = true;
		IsStatic = isStatic;
	}

	public void ClearForces()
	{
		_totalPushbackForce = Vec2.Zero;
		Force = Vec2.Zero;
	}

	public void ClearVelocity()
	{
		Velocity = Vec2.Zero;
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

		float magnitude = CollisionPushbackIntensity * (CollisionPushbackDampening / (CollisionPushbackDampening + distance));
		magnitude -= CollisionPushbackIntensity * (CollisionPushbackDampening / (CollisionPushbackDampening + Radius));
		magnitude += CollisionPushbackMin;
		magnitude = MathF.Min(magnitude, CollisionPushbackClamp);

		Vec2 pushbackForce = direction * magnitude;

		if (this.Enabled && !this.IsStatic)
		{
			this._totalPushbackForce -= pushbackForce;
			this.ApplyForce(-pushbackForce);
		}

		if (other.Enabled && !other.IsStatic)
		{
			other._totalPushbackForce += pushbackForce;
			other.ApplyForce(pushbackForce);
		}
	}

	public override void Tick()
	{
	}

	public void PhysicsTick()
	{
		if (!Enabled || IsStatic) return;

		Vec2 oldPos = new Vec2(Pos.x, Pos.y);

		ClampPushbackForce();

		Vec2 acceleration = Force / Mass;
		Velocity += acceleration;
		Pos += Velocity;

		Velocity *= (1 - Friction);

		ClearForces();

		DeltaPos = Pos - oldPos;
	}

	private void ClampPushbackForce()
	{
		Vec2 pushbackAdjust = Vec2.Zero;
		if (_totalPushbackForce.Magnitude > CollisionPushbackTotalClamp)
		{
			Vec2 clampedPushbackForce = _totalPushbackForce.Unit * CollisionPushbackTotalClamp;
			pushbackAdjust = _totalPushbackForce - clampedPushbackForce;
		}
		Force -= pushbackAdjust;
	}


	// WARNING only works for Radius < map._stepSize / 2
	// public void LimitToBoundaries(Grid<Cell> map)
	public void LimitToBoundaries(Grid<Cell> map)
	{
		if (!Enabled || IsStatic) return;

		Vec2 oldPos = Pos - DeltaPos;

		if (!map.ContainsPosFromWorldSpace(oldPos)) return;
		if (!map.GetObjectAtWorldPos(oldPos).IsWalkable) return;

		Vec2Int oldCellPos = map.CellPosFromWorldSpace(oldPos);

		Vec2 dx = new Vec2(oldPos.x + DeltaPos.x, oldPos.y);
		Vec2 dy = new Vec2(oldPos.x, oldPos.y + DeltaPos.y);

		float error = 0.0001f;

		Vec2 clampPos = Pos;
		if (!map.ContainsPosFromWorldSpace(dx) || !map.GetObjectAtWorldPos(dx).IsWalkable)
		{
			if (DeltaPos.x > 0)
			{
				clampPos.x = map.RightEdgeX(oldCellPos) - error;
			}
			else
			{
				clampPos.x = map.LeftEdgeX(oldCellPos) + error;
			}
		}

		if (!map.ContainsPosFromWorldSpace(dy) || !map.GetObjectAtWorldPos(dy).IsWalkable)
		{
			if (DeltaPos.y > 0)
			{
				clampPos.y = map.UpEdgeY(oldCellPos) - error;
			}
			else
			{
				clampPos.y = map.DownEdgeY(oldCellPos) + error;
			}
		}
		Pos = clampPos;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);

		writer.Write(Pos);
		writer.Write(Force);
		writer.Write(Velocity);
		writer.Write(DeltaPos);

		writer.Write(Radius);
		writer.Write(Friction);
		writer.Write(Mass);

		writer.Write(_enabled);
		writer.Write(IsStatic);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);

		Pos = reader.Read<Vec2>();
		Force = reader.Read<Vec2>();
		Velocity = reader.Read<Vec2>();
		DeltaPos = reader.Read<Vec2>();

		Radius = reader.Read<float>();
		Friction = reader.Read<float>();
		Mass = reader.Read<float>();

		_enabled = reader.Read<bool>();
		IsStatic = reader.Read<bool>();
	}
}

}

