using UnityEngine;

public class OverscanAdjustment : MonoBehaviour
{
	private OptionsDirector options;

	private RectTransform rectTransform;

	public void Awake()
	{
		options = SRSingleton<GameContext>.Instance.OptionsDirector;
		rectTransform = GetComponent<RectTransform>();
	}

	public void Update()
	{
		rectTransform.localScale = Vector3.one - Vector3.one * options.GetOverscanAdjustment();
	}
}
