using RtsEngine.Commands;

namespace RtsEngine.AI
{

public enum RtsAction
{
	TrainWorker,
	TrainKnight,

	BuildBarracks,
	BuildCastle,

	Attack,
	Defend,

	MineGold,

	Wait,
}

public static class RtsActionUtils
{
	public static ICommand ActionToCommand(WorldState state, RtsAction action)
	{

	}
}

}
