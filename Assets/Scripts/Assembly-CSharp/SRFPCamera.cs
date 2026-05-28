using UnityEngine;

public class SRFPCamera : vp_FPCamera
{
	private OptionsDirector optionsDir;

	private Vector4 defaultBobAmp;

	private Vector4 NO_BOB = new Vector4(0f, 0.001f, 0f, 0f);

	protected override void Awake()
	{
		base.Awake();
		optionsDir = SRSingleton<GameContext>.Instance.OptionsDirector;
		GetComponent<Camera>().cullingMask &= -8193;
		defaultBobAmp = BobAmplitude;
	}

	protected override void UpdateBob()
	{
		if (!optionsDir.disableCameraBob)
		{
			BobAmplitude = defaultBobAmp;
		}
		else
		{
			BobAmplitude = NO_BOB;
		}
		base.UpdateBob();
	}
}
