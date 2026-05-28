using System;
using System.Collections.Generic;
using UnityEngine;

public class QuicksilverAmmoReplacer : MonoBehaviour
{
	[Serializable]
	public class WeightedAmmo
	{
		[Tooltip("Identifiable of the ammo to replace with.")]
		public Identifiable.Id id;

		[Tooltip("Weight used when picking a weighted random ammo.")]
		public float weight;

		[Tooltip("Amount of ammo that should be added/replaced with on pickup.")]
		public int count;

		[Tooltip("SFX played when this ammo is picked up by the player.")]
		public SECTR_AudioCue onPickupCue;
	}

	[Tooltip("Energy generator required to be active to replace ammo.")]
	public QuicksilverEnergyGenerator generator;

	[Tooltip("Weighted list of ammo to replace with.")]
	public List<WeightedAmmo> ammo;

	[Tooltip("Time in game hours between ability to trigger this.")]
	public float cooldownHours = 1f;

	[Tooltip("FX to display when the ammo replacer is available. (optional)")]
	public GameObject activeFX;

	private WeightedAmmo picked;

	private TimeDirector timeDir;

	private TutorialDirector tutDir;

	private PlayerState playerState;

	private double unavailUntil;

	private bool? wasReady;

	private Component[] padRenderer;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		tutDir = SRSingleton<SceneContext>.Instance.TutorialDirector;
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		Component[] componentsInChildren = base.transform.parent.gameObject.GetComponentsInChildren<Renderer>();
		padRenderer = componentsInChildren;
		picked = PickNextAmmo();
	}

	public void OnTriggerEnter(Collider col)
	{
		if (IsReady() && PhysicsUtil.IsPlayerMainCollider(col) && playerState.Ammo.ReplaceWithQuicksilverAmmo(picked.id, picked.count))
		{
			if (picked.id == Identifiable.Id.VALLEY_AMMO_1)
			{
				tutDir.MaybeShowPopup(TutorialDirector.Id.RACE_PULSESHOT);
			}
			else
			{
				tutDir.MaybeShowPopup(TutorialDirector.Id.RACE_POWERUP);
			}
			SECTR_AudioSystem.Play(picked.onPickupCue, base.transform.position, loop: false);
			unavailUntil = timeDir.HoursFromNow(cooldownHours);
			picked = PickNextAmmo();
		}
	}

	public void Update()
	{
		bool flag = IsReady();
		if (!wasReady.HasValue || wasReady != flag)
		{
			if (activeFX != null)
			{
				activeFX.SetActive(flag);
			}
			float value = (flag ? 0.5f : 0f);
			Component[] array = padRenderer;
			for (int i = 0; i < array.Length; i++)
			{
				((Renderer)array[i]).material.SetFloat("_SpiralColor", value);
			}
			wasReady = flag;
		}
	}

	private WeightedAmmo PickNextAmmo()
	{
		return Randoms.SHARED.Pick(ammo, (WeightedAmmo a) => a.weight, null);
	}

	private bool IsReady()
	{
		if (generator.GetState() == QuicksilverEnergyGenerator.State.ACTIVE)
		{
			return timeDir.HasReached(unavailUntil);
		}
		return false;
	}
}
