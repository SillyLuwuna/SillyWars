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
	public const int BaseBuildEffort = 10;

	public const int BaseMaxUnitProduction = 4;
	public static readonly List<UnitType> BaseAllowedUnitTypes = new List<UnitType> { UnitType.Knight };

	public override int Height { get; set; }
	public override int Width { get; set; }

	public override int MaxHitPoints => 20;

	public override int BuildEffort { get; set; }

	public override int MaxUnitProduction { get; set; }
	public override List<UnitType> AllowedUnitTypes { get; set; }

	public override ResourceStack Cost => new ResourceStack(Resource.Gold, 50);

	public Barracks(uint ownerId, WorldState world, Vec2Int start) : base(ownerId, world, start, BaseHeight, BaseWidth)
	{
		BuildEffort = BaseBuildEffort;

		MaxUnitProduction = BaseMaxUnitProduction;
		AllowedUnitTypes = BaseAllowedUnitTypes;
	}

	public static Barracks CreateBuilt(uint ownerId, WorldState world, Vec2Int start)
	{
		Barracks barracks = new Barracks(ownerId, world, start);

		barracks.HasBuildingStarted = true;
		barracks.IsBuilt = true;
		barracks.HitPoints = barracks.MaxHitPoints;

		return barracks;
	}

	public override StructureType StructureType => StructureType.Barracks;
}

}
