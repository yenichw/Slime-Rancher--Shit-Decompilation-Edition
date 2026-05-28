using System.Collections.Generic;
using UnityEngine;

public class QuicksilverPlortCollector : SRBehaviour
{
	[Tooltip("Duration, in game minutes, until the plort is collected.")]
	public Range activeMinutes;

	[Tooltip("FX spawned when the countdown to collection begins. (optional)")]
	public GameObject activeFX;

	[Tooltip("FX spawned when the plort is collected. (optional)")]
	public GameObject destroyFX;

	[Tooltip("SFX played when the quicksilver plort begins collection. (optional)")]
	public SECTR_AudioCue onCollectionCue;

	private TimeDirector timeDirector;

	private PediaDirector pediaDirector;

	private double timer;

	public void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		pediaDirector = SRSingleton<SceneContext>.Instance.PediaDirector;
		ResetCollectionTime();
	}

	public void Start()
	{
		if (activeFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(activeFX, base.transform.position, Quaternion.identity);
		}
	}

	public void OnEnable()
	{
		ResetCollectionTime();
	}

	public void Update()
	{
		if (!timeDirector.HasReached(timer))
		{
			return;
		}
		Identifiable component = GetComponent<Identifiable>();
		foreach (KeyValuePair<PlayerState.AmmoMode, Ammo> item in SRSingleton<SceneContext>.Instance.PlayerState.GetAmmoDict())
		{
			if (item.Value.MaybeAddToSlot(component.id, component))
			{
				if (destroyFX != null)
				{
					SRBehaviour.SpawnAndPlayFX(destroyFX, base.transform.position, Quaternion.identity);
				}
				SECTR_AudioSystem.Play(onCollectionCue, base.transform.position, loop: false);
				Destroyer.DestroyActor(base.gameObject, "QuicksilverPlortCollector.Update");
				pediaDirector.MaybeShowPopup(component.id);
				break;
			}
		}
		ResetCollectionTime();
	}

	private void ResetCollectionTime()
	{
		timer = timeDirector.HoursFromNow(activeMinutes.Random() * (1f / 60f));
	}
}
