using System;
using UnityEngine;

public class MedStation : MonoBehaviour
{
	public GameObject medFX;

	public float healthPerEnergy = 1f;

	public float healthPerSecond = 100f;

	private int counts;

	private PlayerState playerState;

	private WaitForChargeup waiter;

	public void Awake()
	{
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		waiter = GetComponentInParent<WaitForChargeup>();
	}

	public void OnTriggerEnter(Collider col)
	{
		if (PhysicsUtil.IsPlayerMainCollider(col))
		{
			counts++;
		}
	}

	public void OnTriggerExit(Collider col)
	{
		if (PhysicsUtil.IsPlayerMainCollider(col))
		{
			counts--;
		}
	}

	public void Update()
	{
		if (waiter.IsWaiting())
		{
			return;
		}
		bool flag = false;
		if (counts > 0)
		{
			int num = Mathf.Max(playerState.GetMaxHealth() - playerState.GetCurrHealth(), playerState.GetCurrRad());
			int num2 = Mathf.CeilToInt((float)playerState.GetCurrEnergy() * healthPerEnergy);
			if (num > 0 && num2 > 0)
			{
				int num3 = Math.Min(Math.Min(num, num2), Mathf.CeilToInt(Time.deltaTime * healthPerSecond));
				if (num3 > 0)
				{
					flag = true;
					playerState.SpendEnergy((float)num3 / healthPerEnergy);
					playerState.Heal(num3);
					playerState.RemoveRads(num3);
				}
			}
		}
		if (flag != medFX.activeSelf)
		{
			medFX.SetActive(flag);
		}
	}
}
