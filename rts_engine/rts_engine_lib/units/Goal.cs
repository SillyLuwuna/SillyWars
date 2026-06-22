namespace RtsEngine.Units
{

// the number of bits necessary to store this will affect UnitState
public enum Goal
{
	None,
	Build,
	Mine,
	Retrieve,
	Attack,
	Walk
}

static class GoalExtensions
{
	public static bool IsNone(this Goal goal) => goal == Goal.None;
	public static bool IsBuild(this Goal goal) => goal == Goal.Build;
	public static bool IsMine(this Goal goal) => goal == Goal.Mine;
	public static bool IsRetrieve(this Goal goal) => goal == Goal.Retrieve;
	public static bool IsAttack(this Goal goal) => goal == Goal.Attack;
	public static bool IsWalk(this Goal goal) => goal == Goal.Walk;
}

}
