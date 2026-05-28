using UnityEngine;

public class NotifyTutorialInBarnArea : MonoBehaviour
{
	private TutorialDirector tutDir;

	public void Awake()
	{
		tutDir = SRSingleton<SceneContext>.Instance.TutorialDirector;
	}

	public void OnTriggerEnter(Collider collider)
	{
		Identifiable component = collider.GetComponent<Identifiable>();
		if (component != null && component.id == Identifiable.Id.PLAYER)
		{
			tutDir.SetInBarnArea(inArea: true);
		}
	}

	public void OnTriggerExit(Collider collider)
	{
		Identifiable component = collider.GetComponent<Identifiable>();
		if (component != null && component.id == Identifiable.Id.PLAYER)
		{
			tutDir.SetInBarnArea(inArea: false);
		}
	}
}
