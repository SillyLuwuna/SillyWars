using RtsEngine.Math;

namespace RtsEngine.Structures
{

public class Castle : BaseStructure
{
	public const int HEIGHT = 3;
	public const int WIDTH = 5;
	public const int MAX_HITPOINTS = 40;
	public const int BUILD_EFFORT = 20;
	public override int Height { get; set; }
	public override int Width { get; set; }

	public override int MaxHitPoints { get; set; }

	public override int BuildEffort { get; set; }

	public Castle(uint ownerId, Vec2Int start) : base(ownerId, start, HEIGHT, WIDTH)
	{
		MaxHitPoints = MAX_HITPOINTS;

		BuildEffort = BUILD_EFFORT;
	}

	public override void Tick()
	{
	}
}

}
