public class DeactivateOnHeld : SRBehaviour
{
	private Vacuumable parent;

	public void Start()
	{
		parent = GetComponentInParent<Vacuumable>();
		if (parent != null)
		{
			parent.onSetHeld += OnSetHeld;
			OnSetHeld(parent.isHeld());
		}
	}

	public void OnDestroy()
	{
		if (parent != null)
		{
			parent.onSetHeld -= OnSetHeld;
			parent = null;
		}
	}

	private void OnSetHeld(bool held)
	{
		base.gameObject.SetActive(!held);
	}
}
