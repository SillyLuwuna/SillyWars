using RtsEngine.Commands;

namespace RtsEngine.AI
{

public class AIPlayer : IRtsPlayer
{
	public AIPlayer()
	{
	}

	public ICommand? MakePlay(WorldState state)
	{
		return null;
	}

	public void GameStarted(WorldState initialState)
	{

	}

	public void GameEnded(WorldState finalState)
	{

	}
}

}
