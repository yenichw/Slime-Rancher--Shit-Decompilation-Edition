public abstract class PhaseableObject : SRBehaviour
{
	public abstract bool ReadyToPhase();

	public abstract void PhaseOut();

	public abstract void PhaseIn();
}
