public struct Vec2
{
	public float x;
	public float y;

	public Vec2(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public float Distance(Vec2 other)
	{
		float dx = other.x - x;
		float dy = other.y = y;
		return MathF.Sqrt(dx * dx + dy * dy);
	}

	public override string ToString()
	{
		return "(" + x + ", " + y + ")";
	}
}
