using UnityEngine;

public class Fashion : MonoBehaviour
{
	public enum Slot
	{
		TOP = 0,
		FRONT = 1
	}

	public Slot slot;

	public GameObject attachPrefab;

	public GameObject attachFX;

	private bool used;

	public void OnCollisionEnter(Collision col)
	{
		if (!used)
		{
			AttachFashions component = col.gameObject.GetComponent<AttachFashions>();
			if (component != null)
			{
				component.Attach(this);
				GetComponent<DestroyOnTouching>().NoteDestroying();
				Destroyer.DestroyActor(base.gameObject, "Fashion.OnCollisionEnter");
				used = true;
			}
		}
	}
}
