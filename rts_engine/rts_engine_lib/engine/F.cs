namespace RtsEngine
{

using System;

public static class F
{
	public const float epsilon = 0.0001f;
	private const float converterMultiplier = 1.0f / epsilon;

	public static bool Eq(float left, float right)
	{
		return Math.Abs(left - right) < epsilon;
	}

	public static bool Lt(float left, float right)
	{
		return left < (right - epsilon);
	}

	public static bool Lte(float left, float right)
	{
		return left <= (right + epsilon);
	}

	public static bool Gt(float left, float right)
	{
		return left > (right + epsilon);
	}

	public static bool Gte(float left, float right)
	{
		return left >= (right - epsilon);
	}

	public static bool Zero(float f)
	{
		return Eq(f, 0.0f);
	}

	public static int Hash(float value)
	{
		return (int)MathF.Round(value * converterMultiplier);
	}
}

}
