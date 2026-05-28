using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DragFloatReactor : SRBehaviour, FloatingReactor
{
	[Tooltip("The factor by which to increase the drag while we're floating.")]
	public float floatDragMultiplier = 25f;

	private float normDrag;

	private Rigidbody body;

	private bool isFloating;

	public void Awake()
	{
		body = GetComponent<Rigidbody>();
		normDrag = body.drag;
	}

	public void SetIsFloating(bool isFloating)
	{
		body.drag = (isFloating ? floatDragMultiplier : 1f) * normDrag;
		this.isFloating = isFloating;
	}

	public bool GetIsFloating()
	{
		return isFloating;
	}

	public static bool IsFloating(GameObject target)
	{
		DragFloatReactor component = target.GetComponent<DragFloatReactor>();
		if (component != null)
		{
			return component.GetIsFloating();
		}
		return false;
	}
}
