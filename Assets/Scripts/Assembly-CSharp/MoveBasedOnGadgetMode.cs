using UnityEngine;

public class MoveBasedOnGadgetMode : MonoBehaviour
{
	public GameObject toMove;

	public Vector2 gadgetModeOnPos;

	public Vector2 gadgetModeOffPos;

	private PlayerState playerState;

	private float lerpVal;

	private const float TRANS_SPEED = 4f;

	public void Awake()
	{
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
	}

	public void Update()
	{
		float num = (playerState.InGadgetMode ? 1f : 0f);
		if (num > lerpVal)
		{
			lerpVal = Mathf.Min(num, lerpVal + 4f * Time.deltaTime);
		}
		else
		{
			lerpVal = Mathf.Max(num, lerpVal - 4f * Time.deltaTime);
		}
		toMove.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(gadgetModeOffPos, gadgetModeOnPos, lerpVal);
	}
}
