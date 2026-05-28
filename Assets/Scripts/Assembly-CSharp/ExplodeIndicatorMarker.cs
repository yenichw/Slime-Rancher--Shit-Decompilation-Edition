public class ExplodeIndicatorMarker : SRBehaviour
{
	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
		if (active)
		{
			SECTR_AudioSource component = GetComponent<SECTR_AudioSource>();
			if (component != null)
			{
				component.Play();
			}
		}
	}
}
