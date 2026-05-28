using UnityEngine;

public class DestroyParentOnDestroy : MonoBehaviour
{
	public void OnDestroy()
	{
		Destroyer.Destroy(base.transform.parent.gameObject, "DestroyParentOnDestroy.OnDestroy");
	}
}
