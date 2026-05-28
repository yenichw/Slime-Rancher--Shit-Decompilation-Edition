public class QuantumVibrationMarker : SRBehaviour
{
	private SECTR_PointSource audioSrc;

	public SECTR_AudioCue vibratingCue;

	public void Awake()
	{
		audioSrc = base.gameObject.GetComponent<SECTR_PointSource>();
	}

	public void PlayVibrating()
	{
		PlayCue(vibratingCue);
	}

	public void PlayCalm()
	{
		PlayCue(null);
	}

	private void PlayCue(SECTR_AudioCue cue)
	{
		audioSrc.Cue = cue;
		if (cue != null)
		{
			audioSrc.Play();
		}
		else
		{
			audioSrc.Stop(stopImmediately: false);
		}
	}
}
