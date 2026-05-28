using Assets.Script.Util.Extensions;
using UnityEngine;

public class BiteEventAggregator : MonoBehaviour
{
	public delegate void EnableBiteEvent();

	public delegate void DisableBiteEvent();

	public delegate void SpawnBubblesEvent();

	private Animator bodyAnim;

	public event EnableBiteEvent OnEnableBite;

	public event DisableBiteEvent OnDisableBite;

	public event SpawnBubblesEvent OnSpawnBubbles;

	public void Awake()
	{
		bodyAnim = base.gameObject.GetRequiredComponentInChildren<Animator>();
	}

	public void EnableBiteModel()
	{
		if (this.OnEnableBite != null)
		{
			this.OnEnableBite();
		}
	}

	public void DisableBiteModel()
	{
		if (this.OnDisableBite != null)
		{
			this.OnDisableBite();
		}
	}

	public void SpawnBubbles()
	{
		if (this.OnSpawnBubbles != null)
		{
			this.OnSpawnBubbles();
		}
	}

	public bool IsBiteAnimationStateActive()
	{
		if (!bodyAnim.GetCurrentAnimatorStateInfo(0).IsName("Bite"))
		{
			return bodyAnim.GetCurrentAnimatorStateInfo(0).IsName("BiteQuick");
		}
		return true;
	}
}
