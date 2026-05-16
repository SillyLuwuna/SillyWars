public struct Vec2Int
{
	public int x;
	public int y;

	public Vec2Int(int x, int y)
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
