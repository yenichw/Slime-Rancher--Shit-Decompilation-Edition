using UnityEngine;

public class PediaListingUI : MonoBehaviour
{
	public PediaDirector.Id id;

	private PediaUI ui;

	public void Start()
	{
		ui = GetComponentInParent<PediaUI>();
	}

	public void OnClick()
	{
		ui.SelectEntry(id, selectTab: false, id);
	}
}
