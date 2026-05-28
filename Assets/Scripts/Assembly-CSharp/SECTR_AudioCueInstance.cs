using UnityEngine;

public struct SECTR_AudioCueInstance : SECTR_IAudioInstance
{
	private SECTR_IAudioInstance internalInstance;

	private int generation;

	private bool loops;

	public bool Loops => loops;

	public int Generation => generation;

	public bool Active
	{
		get
		{
			if (internalInstance != null && generation == internalInstance.Generation)
			{
				return internalInstance.Active;
			}
			return false;
		}
	}

	public Vector3 Position
	{
		get
		{
			if (!Active)
			{
				return Vector3.zero;
			}
			return internalInstance.Position;
		}
		set
		{
			if (Active)
			{
				internalInstance.Position = value;
			}
		}
	}

	public Vector3 LocalPosition
	{
		get
		{
			if (!Active)
			{
				return Vector3.zero;
			}
			return internalInstance.LocalPosition;
		}
		set
		{
			if (Active)
			{
				internalInstance.LocalPosition = value;
			}
		}
	}

	public float Volume
	{
		get
		{
			if (!Active)
			{
				return 0f;
			}
			return internalInstance.Volume;
		}
		set
		{
			if (Active)
			{
				internalInstance.Volume = value;
			}
		}
	}

	public float Pitch
	{
		get
		{
			if (!Active)
			{
				return 1f;
			}
			return internalInstance.Pitch;
		}
		set
		{
			if (Active)
			{
				internalInstance.Pitch = value;
			}
		}
	}

	public bool Mute
	{
		get
		{
			if (!Active)
			{
				return false;
			}
			return internalInstance.Mute;
		}
		set
		{
			if (Active)
			{
				internalInstance.Mute = value;
			}
		}
	}

	public float TimeSeconds
	{
		get
		{
			if (!Active)
			{
				return 0f;
			}
			return internalInstance.TimeSeconds;
		}
		set
		{
			if (Active)
			{
				internalInstance.TimeSeconds = value;
			}
		}
	}

	public int TimeSamples
	{
		get
		{
			if (!Active)
			{
				return 0;
			}
			return internalInstance.TimeSamples;
		}
		set
		{
			if (Active)
			{
				internalInstance.TimeSamples = value;
			}
		}
	}

	public SECTR_AudioCueInstance(SECTR_IAudioInstance internalInstance, int generation, bool loops)
	{
		this.internalInstance = internalInstance;
		this.generation = generation;
		this.loops = loops;
	}

	public void Stop(bool stopImmediately)
	{
		if (Active)
		{
			internalInstance.Stop(stopImmediately);
		}
	}

	public void Pause(bool paused)
	{
		if (Active)
		{
			internalInstance.Pause(paused);
		}
	}

	public void ForceInfinite()
	{
		if (Active)
		{
			internalInstance.ForceInfinite();
		}
	}

	public void ForceOcclusion(bool occluded)
	{
		if (Active)
		{
			internalInstance.ForceOcclusion(occluded);
		}
	}

	public void SkipFadeIn()
	{
		if (Active)
		{
			internalInstance.SkipFadeIn();
		}
	}

	public SECTR_IAudioInstance GetInternalInstance()
	{
		return internalInstance;
	}

	public static implicit operator bool(SECTR_AudioCueInstance x)
	{
		return x.Active;
	}
}
