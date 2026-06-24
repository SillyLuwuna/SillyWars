namespace RtsEngine.Resources
{

public struct ResourceStack
{
	public Resource Resource;
	public int Amount;

	public ResourceStack(Resource resource, int amount)
	{
		Resource = resource;
		Amount = amount;
	}
}

}
