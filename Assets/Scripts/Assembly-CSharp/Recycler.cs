using UnityEngine;

public class Recycler : MonoBehaviour, DestroyRequestHandler
{
	public delegate void RecycleEvent(GameObject obj);

	private bool hasRecycled;

	public RecycleEvent OnBeforeRecycle;

	public RecycleEvent OnAfterRecycle;

	public ObjectPool pool;

	public void OnEnable()
	{
		hasRecycled = false;
	}

	public void OnDestroyRequest(GameObject obj)
	{
		if (!hasRecycled)
		{
			hasRecycled = true;
			if (OnBeforeRecycle != null)
			{
				OnBeforeRecycle(obj);
			}
			pool.Recycle(obj);
			if (OnAfterRecycle != null)
			{
				OnAfterRecycle(obj);
			}
		}
	}
}
