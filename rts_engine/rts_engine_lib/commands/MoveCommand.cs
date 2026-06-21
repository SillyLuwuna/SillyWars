using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Math;

namespace RtsEngine.Commands
{

public class MoveCommand : EntityCommand<MoveCommandArgs>
{
	private uint currEntity;
	private float radius;

	public MoveCommand(uint playerId, MoveCommandArgs args) : base(playerId, args)
	{
		Init();
	}

	private void Init()
	{
		currEntity = 0;
	}

	public override void SerializeFields(SerializerWriter writer)
	{
		base.SerializeFields(writer);
	}

	public override void DeserializeFields(SerializerReader reader)
	{
		base.DeserializeFields(reader);

		Init();
	}

	protected override bool ValidateEntity(WorldState state, Entity? entity)
	{
		if (!base.ValidateEntity(state, entity)) return false;

		if (!(entity is IMovable)) return false;
		return true;
	}

	protected override void ExecuteSpecific(WorldState state)
	{
		base.ExecuteSpecific(state);

		// int i = EntityRefs.Count;
		// Vec2 goal = _args.Goal;
	}

	protected override void ExecuteEntity(WorldState state, Entity entity)
	{
		// based on number of units and radius calculate density (ideal unit per m^2), this number could be adjusted
		// based on the density, get a random soft-goal (uniform probability?)
		// if the soft-goal falls outside the map, clamp it to the nearest edge starting from the main goal (or just select a different random one, bad for large densities and small spaces)
		((IMovable)entity).Move(state.Map, _args.Goal);
	}
}

}
