using UnityEngine;

public class ConvertToCurrency : MonoBehaviour
{
	[Tooltip("Delay, in real-time seconds, before the currency is granted.")]
	public float delay = 0.25f;

	[Tooltip("Amount of currency to grant.")]
	public int amount;

	public GameObject destroyFX;

	private PlayerState playerState;

	private float convertTime;

	private const float ANIMATION_DURATION = 4f;

	private float destroyTime;

	public void Awake()
	{
		convertTime = Time.time + delay;
		destroyTime = convertTime + 4f;
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		playerState.AddCurrency(amount, PlayerState.CoinsType.NONE);
		playerState.AddCurrencyDisplayDelta(-amount);
	}

	private void Update()
	{
		if (Time.time >= convertTime && amount > 0)
		{
			SRSingleton<PopupElementsUI>.Instance.CreateCoinsPopup(amount, PlayerState.CoinsType.NORM);
			playerState.AddCurrencyDisplayDelta(amount);
			amount = 0;
		}
		if (Time.time >= destroyTime)
		{
			if (destroyFX != null)
			{
				Object.Instantiate(destroyFX, base.gameObject.transform.position, base.gameObject.transform.rotation);
			}
			Destroyer.Destroy(base.gameObject, "ConvertToCurrency.Update");
		}
	}
}
