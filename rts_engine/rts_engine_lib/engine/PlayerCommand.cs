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
	// private uint _entityId;
	private CommandType _command;
	private byte[] _args;

	public PlayerCommand(uint playerId, CommandType command, byte[] args)
	{
		_playerId = playerId;
		// _entityId = entityId;
		_command = command;
		_args = args;
	}

	public void SerializeFields(SerializerWriter writer)
	{
		writer.Write(_playerId);
		writer.Write(_command);
		writer.Write(_args);

		// writer.Write(_playerId);
		// writer.Write(_entityId);
		// writer.Write((byte)_command);
		// writer.Write(_args.Length);
		// writer.Write(_args);
	}

	public void DeserializeFields(SerializerReader reader)
	{
		_playerId = reader.Read<uint>();
		_command = reader.Read<CommandType>();
		_args = reader.Read<byte[]>();

		// _playerId = reader.ReadUInt32();
		// // _entityId = reader.ReadUInt32();
		// _command = (CommandType)reader.ReadByte();
		// int argLen = reader.ReadInt32();
		// _args = reader.ReadBytes(argLen);
	}

	public void Execute(WorldState state)
	{
		// Entity? entity = state.GetEntity(_entityId);
		// if (entity == null) return;
		// if (entity.OwnerId != _playerId) return;

		using MemoryStream ms = new MemoryStream(_args);
		using SerializerReader reader = new SerializerReader(ms);

		switch (_command)
		{
			case CommandType.Move:
				ExecuteMove(state, reader);
				break;
			case CommandType.Halt:
				ExecuteHalt(state, reader);
				break;
			default:
				throw new NotSupportedException("Unknown command");
		}
	}

	private void ExecuteMove(WorldState state, SerializerReader argsReader)
	{
		// uint entityId = argsReader.ReadUInt32();
		uint entityId = argsReader.Read<uint>();
		Entity? entity = state.GetEntity(entityId);

		if (entity == null) return;
		if (entity.OwnerId != _playerId) return;
		if (!(entity is IMovable correctEntity)) return;

		Vec2 goal = argsReader.Read<Vec2>();
		// Vec2 goal = Serializer.FromBytes<Vec2>(_args);
		// Vec2 goal = Serializer.Deserialize<Vec2>(argsReader);

		correctEntity.Move(state.Map, goal);
	}

	private void ExecuteHalt(WorldState state, SerializerReader argsReader)
	{
		uint entityId = argsReader.Read<uint>();
		Entity? entity = state.GetEntity(entityId);
		if (!(entity is IMovable correctEntity)) return;

		correctEntity.Halt();
	}

	public static PlayerCommand MoveCommand(Entity entity, Vec2 goal)
	{
		using MemoryStream ms = new MemoryStream();
		using SerializerWriter writer = new SerializerWriter(ms);

		writer.Write(entity.Id);
		writer.Write(goal);

		byte[] args = ms.ToArray();

		// byte[] args = Serializer.ToBytes<Vec2>(goal);

		return new PlayerCommand(0, CommandType.Move, args);
		// return new PlayerCommand(0, entity.Id, CommandType.Move, args);
	}

	public static PlayerCommand HaltCommand(Entity entity)
	{
		using MemoryStream ms = new MemoryStream();
		using SerializerWriter writer = new SerializerWriter(ms);

		writer.Write(entity.Id);

		byte[] args = ms.ToArray();

		return new PlayerCommand(0, CommandType.Halt, args);
		// return new PlayerCommand(0, entity.Id, CommandType.Halt, new byte[] {});
	}
}

}
