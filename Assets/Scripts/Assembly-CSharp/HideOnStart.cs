using UnityEngine;

public class HideOnStart : SRBehaviour
{
	private void Start()
	{
		GetComponent<Renderer>().enabled = false;
	}
}
