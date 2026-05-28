using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HoldToPress : MonoBehaviour
{
	public class HoldCompleteEvent : UnityEvent
	{
	}

	public float holdTime;

	public Image holdToPressPrefab;

	private Image holdToPress;

	public UnityEvent OnHoldComplete;

	private bool holdComplete;

	private const float INITIAL_FILL_AMOUNT = 0.25f;

	public void OnEnable()
	{
		holdToPress = Object.Instantiate(holdToPressPrefab, base.transform);
		holdToPress.fillAmount = 0.25f;
		holdComplete = false;
	}

	public void OnDisable()
	{
		Destroyer.Destroy(holdToPress.gameObject, "HoldToPress.OnDisable");
		holdToPress = null;
		holdComplete = false;
	}

	public void Update()
	{
		if (holdToPress != null)
		{
			if (holdToPress.fillAmount < 1f)
			{
				holdToPress.fillAmount += Time.unscaledDeltaTime / holdTime;
			}
			else if (!holdComplete)
			{
				holdComplete = true;
				OnHoldComplete.Invoke();
			}
		}
	}
}
