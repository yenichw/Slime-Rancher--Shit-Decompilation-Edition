using UnityEngine;

public class PollenSource : SRBehaviour
{
	public void OnTriggerEnter(Collider col)
	{
		if (col.isTrigger)
		{
			return;
		}
		Identifiable component = col.gameObject.GetComponent<Identifiable>();
		if (component != null && !Identifiable.IsAllergyFree(component.id))
		{
			SlimeEmotions component2 = col.gameObject.GetComponent<SlimeEmotions>();
			if (component2 != null)
			{
				component2.AddPollenSource();
			}
			CauseSneeze(col.gameObject);
		}
	}

	public void OnTriggerExit(Collider col)
	{
		if (col.isTrigger)
		{
			return;
		}
		Identifiable component = col.gameObject.GetComponent<Identifiable>();
		if (component != null && !Identifiable.IsAllergyFree(component.id))
		{
			SlimeEmotions component2 = col.gameObject.GetComponent<SlimeEmotions>();
			if (component2 != null)
			{
				component2.RemovePollenSource();
			}
		}
	}

	private void CauseSneeze(GameObject gameObject)
	{
		SlimeFaceAnimator component = gameObject.GetComponent<SlimeFaceAnimator>();
		if (component != null)
		{
			component.SetTrigger("triggerSneeze");
		}
	}
}
