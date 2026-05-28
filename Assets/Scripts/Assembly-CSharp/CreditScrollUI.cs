using UnityEngine;

public class CreditScrollUI : MonoBehaviour
{
	public void OnEnable()
	{
	}

	public void OnDisable()
	{
		_ = SRSingleton<GameContext>.Instance != null;
	}
}
