using UnityEngine;

public class SectrInitializer : MonoBehaviour
{
	public GameObject sectorRoot;

	public void Start()
	{
		SECTR_Chunk component = GetComponent<SECTR_Chunk>();
		component.SetRoot(sectorRoot);
		component.CheckReferences();
	}
}
