namespace RtsEngine.Math
{

using System;

public static class F
{
	public const float Epsilon = 0.0001f;
	private const float ConverterMultiplier = 1.0f / Epsilon;

	public static bool Eq(float left, float right)
	{
		return Math.Abs(left - right) < Epsilon;
	}

	public static bool Lt(float left, float right)
	{
		return left < (right - Epsilon);
	}

	public static bool Lte(float left, float right)
	{
		return left <= (right + Epsilon);
	}

	public static bool Gt(float left, float right)
	{
		return left > (right + Epsilon);
	}

	public static bool Gte(float left, float right)
	{
		return left >= (right - Epsilon);
	}

	public static bool Zero(float f)
	{
		return Eq(f, 0.0f);
	}

	public static int Hash(float value)
	{
		return (int)MathF.Round(value * ConverterMultiplier);
	}
}

}
