using System;
using System.IO;
using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Math;

namespace RtsEngine
{

public class PlayerCommand : ISerializable
{
	public uint _playerId;
	private uint _entityId;
	private CommandType _command;
	private byte[] _args;

	public PlayerCommand(uint playerId, uint entityId, CommandType command, byte[] args)
	{
		_playerId = playerId;
		_entityId = entityId;
		_command = command;
		_args = args;
	}

	public void SerializeFields(BinaryWriter writer)
	{
		writer.Write(_playerId);
		writer.Write(_entityId);
		writer.Write((byte)_command);
		writer.Write(_args.Length);
		writer.Write(_args);
	}

	public void DeserializeFields(BinaryReader reader)
	{
		_playerId = reader.ReadUInt32();
		_entityId = reader.ReadUInt32();
		_command = (CommandType)reader.ReadByte();
		int argLen = reader.ReadInt32();
		_args = reader.ReadBytes(argLen);
	}

	public void Execute(WorldState state)
	{
		Entity? entity = state.GetEntity(_entityId);
		if (entity == null) return;
		if (entity.OwnerId != _playerId) return;

		switch (_command)
		{
			case CommandType.Move:
				ExecuteMove(state, entity);
				break;
			case CommandType.Halt:
				ExecuteHalt(state, entity);
				break;
			default:
				throw new NotSupportedException("Unknown command");
		}
	}

	private void ExecuteMove(WorldState state, Entity entity)
	{
		if (!(entity is IMovable correctEntity)) return;

		Vec2 goal = Serializer.FromBytes<Vec2>(_args);

		correctEntity.Move(state.Map, goal);
	}

	private void ExecuteHalt(WorldState state, Entity entity)
	{
		if (!(entity is IMovable correctEntity)) return;

		correctEntity.Halt();
	}

	public static PlayerCommand MoveCommand(Entity entity, Vec2 goal)
	{
		byte[] args = Serializer.ToBytes<Vec2>(goal);

		return new PlayerCommand(0, entity.Id, CommandType.Move, args);
	}

	public static PlayerCommand HaltCommand(Entity entity)
	{
		return new PlayerCommand(0, entity.Id, CommandType.Halt, new byte[] {});
	}
}

}
