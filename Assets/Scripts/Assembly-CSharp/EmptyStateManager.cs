public class EmptyStateManager<VComp> : BaseStateManager<EmptyState, VComp> where VComp : vp_Component
{
	public EmptyStateManager(VComp managedComponent)
		: base(managedComponent)
	{
		states = new EmptyState[1];
		AddState(new EmptyState("Default"), 0);
	}

	public override void ApplyState(EmptyState state)
	{
	}
}
