using System;

public static class GameMath
{
	public const float epsilon = 0.0001f;
	private const float converterMultiplier = 1.0f / epsilon;

	public static bool FloatCompare(float left, float right)
	{
		return Math.Abs(left - right) < epsilon;
	}

	public static int RoundToEpsilonHash(float value)
	{
		// return MathF.Round(toRound / epsilon) * epsilon;
		return (int)MathF.Round(value * converterMultiplier);
	}
}
