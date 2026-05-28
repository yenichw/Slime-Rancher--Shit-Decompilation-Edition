using UnityEngine;

public class EventTriggerCustom : MonoBehaviour
{
	public GameObject objectToSpawn;

	public SECTR_AudioCue audioToPlay;

	private SECTR_AudioCueInstance cueInstance;

	public void SpawnObjectCustom(AnimationEvent animationEvent)
	{
		string stringParameter = animationEvent.stringParameter;
		Transform transform = base.transform.Find(stringParameter);
		if ((bool)transform)
		{
			Spawn(objectToSpawn, transform);
			PlayAudio(audioToPlay, transform);
		}
		else
		{
			Spawn(objectToSpawn, base.transform);
			PlayAudio(audioToPlay, base.transform);
		}
	}

	private void Spawn(GameObject obj, Transform targetTransform)
	{
		Object.Instantiate(obj, targetTransform.position, targetTransform.rotation);
	}

	private void PlayAudio(SECTR_AudioCue cue, Transform location)
	{
		if (cue != null)
		{
			if (cueInstance.Active)
			{
				cueInstance.Stop(stopImmediately: true);
			}
			cueInstance = SECTR_AudioSystem.Play(cue, location.position, loop: false);
		}
	}

	public void OnDestroy()
	{
		if (cueInstance.Active)
		{
			cueInstance.Stop(stopImmediately: true);
		}
	}
}
