using UnityEngine;

public class EventTrigger : MonoBehaviour
{
	public void PrintEvent(string LOG)
	{
		Debug.Log(LOG);
	}

	public void PlayAudio(AudioClip AUD)
	{
	}

	public void SpawnObject(AnimationEvent animationEvent)
	{
		string stringParameter = animationEvent.stringParameter;
		GameObject original = (GameObject)animationEvent.objectReferenceParameter;
		Transform transform = base.transform.Find(stringParameter);
		if ((bool)transform)
		{
			Object.Instantiate(original, transform.position, transform.rotation);
		}
		else
		{
			Object.Instantiate(original, base.transform.position, base.transform.rotation);
		}
	}
}
