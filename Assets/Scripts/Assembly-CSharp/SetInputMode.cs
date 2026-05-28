using UnityEngine;

public class SetInputMode : MonoBehaviour
{
	[Tooltip("InputMode to set.")]
	public SRInput.InputMode mode;

	private SRInput input;

	public void Awake()
	{
		input = SRInput.Instance;
	}

	public void OnEnable()
	{
		input.SetInputMode(mode, base.gameObject.GetInstanceID());
	}

	public void OnDisable()
	{
		input.ClearInputMode(base.gameObject.GetInstanceID());
	}
}
