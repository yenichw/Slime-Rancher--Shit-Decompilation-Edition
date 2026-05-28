public class EchoNoteGordoAnimator : SRAnimator<EchoNoteGordo>
{
	public void OnAnimationEvent_Popped()
	{
		base.parent.OnAnimationEvent_Popped();
	}
}
