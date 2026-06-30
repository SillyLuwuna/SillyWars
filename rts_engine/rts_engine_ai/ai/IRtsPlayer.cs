using RtsEngine.Commands;

namespace RtsEngine.AI
{

public interface IRtsPlayer
{
	public ICommand? MakePlay(WorldState state, ulong currTick);
	public void GameStarted(WorldState state);
	public void GameEnded(WorldState state, ulong currTick);
}

}
