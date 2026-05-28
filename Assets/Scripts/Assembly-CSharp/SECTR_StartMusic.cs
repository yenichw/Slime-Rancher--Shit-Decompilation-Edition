using UnityEngine;

[AddComponentMenu("SECTR/Audio/SECTR Start Music")]
public class SECTR_StartMusic : MonoBehaviour
{
	[SECTR_ToolTip("The music to play on Start.")]
	public SECTR_AudioCue Cue;

	private void Start()
	{
		SECTR_AudioSystem.PlayMusic(Cue);
		Destroyer.Destroy(this, "SECTR_StartMusic.Start");
	}
}
