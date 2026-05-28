using MonomiPark.SlimeRancher.DataModel;

public class SlimeAudio : SRBehaviour, ActorModel.Participant
{
	public SlimeSounds slimeSounds;

	private SECTR_PointSource source;

	private SlimeModel slimeModel;

	public void Awake()
	{
		source = GetComponent<SECTR_PointSource>();
	}

	public void Play(SECTR_AudioCue cue)
	{
		if (cue != null && (slimeModel == null || !slimeModel.isFeral || !slimeSounds.SuppressIfFeral(cue)))
		{
			source.Cue = cue;
			source.Play();
		}
	}

	public void InitModel(ActorModel model)
	{
	}

	public void SetModel(ActorModel model)
	{
		slimeModel = model as SlimeModel;
	}
}
