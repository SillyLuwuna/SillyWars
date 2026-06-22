namespace RtsEngine.EntityProperties
{

public interface IAttacker : IStateful
{
	public float AttackRange { get; }
	public int AttackDamage { get; }
	public int AttackSpeed { get; }
	public float ChaseDistance { get; }
	public float AggroRange { get; set; }

	public void Attack(IDestroyable entity);

	public void SetAggro(bool aggro);
}

}

