using System;
using UnityEngine;

public class Line
{
	private float _a;
	private float _b;
	private float _c;

	public Line(Vector2 first, Vector2 second)
	{
		_a = first.y - second.y;
		_b = second.x - first.x;
		_c = first.x * second.y - second.x * first.y;
	}

	public float Fx(float y)
	{
		if (GameMath.FloatEq(_a, 0.0f))
		{
			throw new DivideByZeroException("cannot calculate f(y) when a = 0");
		}

		return - (_b * y + _c) / _a;
	}

	public float Fy(float x)
	{
		if (GameMath.FloatEq(_b, 0.0f))
		{
			throw new DivideByZeroException("cannot calculate f(x) when b = 0");
		}

		return - (_a * x + _c) / _b;
	}
	
	public bool IsHorizontal => GameMath.FloatEq(_a, 0.0f);
	public bool IsVertical => GameMath.FloatEq(_b, 0.0f);

	public override string ToString()
	{
		return "(" + _a + ")x + (" + _b + ")y + (" + _c + ")";
	}
}
