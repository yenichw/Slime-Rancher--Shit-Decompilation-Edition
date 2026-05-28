using UnityEngine;

public class FashionRemover : MonoBehaviour
{
	public GameObject removeFX;

	private bool used;

	public void OnCollisionEnter(Collision col)
	{
		if (!used)
		{
			AttachFashions component = col.gameObject.GetComponent<AttachFashions>();
			if (component != null)
			{
				component.DetachAll(this);
				GetComponent<DestroyOnTouching>().NoteDestroying();
				Destroyer.DestroyActor(base.gameObject, "FashionRemover.OnCollisionEnter");
				used = true;
			}
		}
	}
}
