using UnityEngine;

public abstract class SECTR_AudioSource : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	protected float volume = 1f;

	[SerializeField]
	[HideInInspector]
	protected float pitch = 1f;

	[SECTR_ToolTip("The Cue to play from this source.", null, false)]
	public SECTR_AudioCue Cue;

	[SECTR_ToolTip("If the Cue should be forced to loop when playing.")]
	public bool Loop = true;

	[SECTR_ToolTip("Should the Cue auto-play when created.")]
	public bool PlayOnStart = true;

	[SECTR_ToolTip("Should looping cues restart on enabled.")]
	public bool RestartLoopsOnEnabled = true;

	public float Volume
	{
		get
		{
			return volume;
		}
		set
		{
			if (volume != value)
			{
				volume = Mathf.Clamp01(value);
				OnVolumePitchChanged();
			}
		}
	}

	public float Pitch
	{
		get
		{
			return pitch;
		}
		set
		{
			if (pitch != value)
			{
				pitch = Mathf.Clamp(value, 0f, 2f);
				OnVolumePitchChanged();
			}
		}
	}

	public abstract bool IsPlaying { get; }

	public abstract void Play();

	public abstract void Stop(bool stopImmediately);

	protected virtual void Start()
	{
		if (Application.isPlaying && PlayOnStart)
		{
			Play();
		}
	}

	protected virtual void OnDisable()
	{
		if (Application.isPlaying)
		{
			Stop(stopImmediately: true);
		}
	}

	protected virtual void OnEnable()
	{
		if (Application.isPlaying && Cue != null && RestartLoopsOnEnabled && (Cue.Loops || Loop))
		{
			Play();
		}
	}

	protected abstract void OnVolumePitchChanged();
}
