using System.Collections.Generic;
using RtsEngine.Math;
using RtsEngine.Resources;
using RtsEngine.Units;

namespace RtsEngine.Structures
{

public class Barracks : UnitProducer
{
	public const int BaseHeight = 3;
	public const int BaseWidth = 3;
	public const int BaseMaxHitpoints = 20;
	public const int BaseBuildEffort = 10;

	public const int BaseMaxUnitProduction = 4;
	public static readonly List<UnitType> BaseAllowedUnitTypes = new List<UnitType> { UnitType.Knight };

	public override int Height { get; set; }
	public override int Width { get; set; }

	public override int MaxHitPoints { get; set; }

	public override int BuildEffort { get; set; }

	public override int MaxUnitProduction { get; set; }
	public override List<UnitType> AllowedUnitTypes { get; set; }

	public override ResourceStack Cost { get => new ResourceStack(Resource.Gold, 50); }

	public Barracks(uint ownerId, Vec2Int start) : base(ownerId, start, BaseHeight, BaseWidth)
	{
		MaxHitPoints = BaseMaxHitpoints;

		BuildEffort = BaseBuildEffort;

		MaxUnitProduction = BaseMaxUnitProduction;
		AllowedUnitTypes = BaseAllowedUnitTypes;
	}

	public static Barracks CreateBuilt(uint ownerId, Vec2Int start)
	{
		Barracks barracks = new Barracks(ownerId, start);

		barracks.HasBuildingStarted = true;
		barracks.IsBuilt = true;

		return barracks;
	}
}

}
