using RtsEngine.Data;

namespace RtsEngine.Commands
{

public interface ICommand : ISerializable
{
	public uint PlayerId { get; set; }
	public void Execute(WorldState state);
}

}
