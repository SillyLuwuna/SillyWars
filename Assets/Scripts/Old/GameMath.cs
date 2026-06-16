using System;
using UnityEngine;

public static class GameMath
{
	public const float epsilon = 0.0001f;
	private const float converterMultiplier = 1.0f / epsilon;

	public static bool FloatEq(float left, float right)
	{
		return Math.Abs(left - right) < epsilon;
	}

	public static bool FloatLte(float left, float right)
	{
		return left <= (right + epsilon);
	}

	public static bool FloatLt(float left, float right)
	{
		return left < (right - epsilon);
	}

	public static bool FloatGt(float left, float right)
	{
		return left > (right + epsilon);
	}

	public static bool FloatGte(float left, float right)
	{
		return left >= (right - epsilon);
	}

	public static int RoundToEpsilonHash(float value)
	{
		return (int)MathF.Round(value * converterMultiplier);
	}

	public static bool Vector2Compare(Vector2 left, Vector2 right)
	{
		return FloatEq(left.x, right.x) && FloatEq(left.y, right.y);
	}
}
