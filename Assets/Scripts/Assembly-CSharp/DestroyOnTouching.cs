using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnTouching : SRBehaviour
{
	[Tooltip("How long, in hours, we can contact one or more non-water objects before poofing.")]
	public float hoursOfContactAllowed;

	[Tooltip("When we poof, how large an area is watered.")]
	public float wateringRadius;

	[Tooltip("Amount to water each thing in radius when we poof.")]
	public float wateringUnits = 3f;

	[Tooltip("The effect to play when we poof.")]
	public GameObject destroyFX;

	[Tooltip("Should we destroy only if touching a non-water object?")]
	public bool touchingWaterOkay = true;

	[Tooltip("Should we destroy only if touching a non-ash object? Note: Does not include toys.")]
	public bool touchingAshOkay;

	[Tooltip("Should we destroy if touching an actor even when in the water. Note: Does not include toys.")]
	public bool reactToActors;

	[Tooltip("The type of liquid we should spread on destruction.")]
	public Identifiable.Id liquidType = Identifiable.Id.WATER_LIQUID;

	private double destroyAt = double.PositiveInfinity;

	private HashSet<GameObject> destructiveContacts = new HashSet<GameObject>();

	private HashSet<LiquidSource> waterSources = new HashSet<LiquidSource>();

	private HashSet<AshSafetyZone> ashSources = new HashSet<AshSafetyZone>();

	private TimeDirector timeDir;

	private bool destroying;

	private SlimeSubbehaviourPlexer plexer;

	private bool hasPlexer;

	private SlimeAppearanceApplicator slimeAppearanceApplicator;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		plexer = base.gameObject.GetComponent<SlimeSubbehaviourPlexer>();
		hasPlexer = plexer != null;
		slimeAppearanceApplicator = GetComponent<SlimeAppearanceApplicator>();
		if (slimeAppearanceApplicator != null)
		{
			slimeAppearanceApplicator.OnAppearanceChanged += UpdateDestroyFX;
			if (slimeAppearanceApplicator.Appearance != null)
			{
				UpdateDestroyFX(slimeAppearanceApplicator.Appearance);
			}
		}
	}

	public void FixedUpdate()
	{
		if (timeDir.HasReached(destroyAt))
		{
			DestroyAndWater();
		}
	}

	public void NoteDestroying()
	{
		destroying = true;
	}

	private void DestroyAndWater()
	{
		if (destroying)
		{
			return;
		}
		destroying = true;
		if (destroyFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(destroyFX, base.transform.position, base.transform.rotation);
		}
		if (wateringRadius > 0f)
		{
			SphereOverlapTrigger.CreateGameObject(base.transform.position, wateringRadius, delegate(IEnumerable<Collider> colliders)
			{
				HashSet<LiquidConsumer> hashSet = new HashSet<LiquidConsumer>();
				foreach (Collider collider in colliders)
				{
					LiquidConsumer[] array = ((!collider.isTrigger) ? collider.gameObject.GetComponentsInParent<LiquidConsumer>() : collider.gameObject.GetComponents<LiquidConsumer>());
					LiquidConsumer[] array2 = array;
					foreach (LiquidConsumer item in array2)
					{
						hashSet.Add(item);
					}
				}
				foreach (LiquidConsumer item2 in hashSet)
				{
					item2.AddLiquid(liquidType, wateringUnits);
				}
			}, 4);
		}
		Destroyer.DestroyActor(base.gameObject, "DestroyOnTouching.DestroyAndWater");
	}

	public void OnCollisionEnter(Collision col)
	{
		if (IsDestructiveContact(col) && destructiveContacts.Add(col.gameObject))
		{
			UpdateDestroyTime();
		}
	}

	public void OnCollisionExit(Collision col)
	{
		if (IsDestructiveContact(col) && destructiveContacts.Remove(col.gameObject))
		{
			UpdateDestroyTime();
		}
	}

	public void OnTriggerEnter(Collider col)
	{
		LiquidSource component = col.gameObject.GetComponent<LiquidSource>();
		if (component != null && Identifiable.IsWater(component.liquidId) && waterSources.Add(component))
		{
			UpdateDestroyTime();
		}
		AshSafetyZone component2 = col.gameObject.GetComponent<AshSafetyZone>();
		if (component2 != null && ashSources.Add(component2))
		{
			UpdateDestroyTime();
		}
	}

	public void OnTriggerExit(Collider col)
	{
		LiquidSource component = col.gameObject.GetComponent<LiquidSource>();
		if (component != null && Identifiable.IsWater(component.liquidId) && waterSources.Remove(component))
		{
			UpdateDestroyTime();
		}
		AshSafetyZone component2 = col.gameObject.GetComponent<AshSafetyZone>();
		if (component2 != null && ashSources.Remove(component2))
		{
			UpdateDestroyTime();
		}
	}

	private void UpdateDestroyFX(SlimeAppearance appearance)
	{
		if (appearance.DeathAppearance != null)
		{
			destroyFX = appearance.DeathAppearance.deathFX;
		}
	}

	private void UpdateDestroyTime()
	{
		destructiveContacts.RemoveWhere((GameObject c) => c == null);
		waterSources.RemoveWhere((LiquidSource w) => w == null || w.gameObject == null);
		ashSources.RemoveWhere((AshSafetyZone a) => a == null || a.gameObject == null);
		bool num;
		if (!hasPlexer)
		{
			num = destructiveContacts.Count == 0;
		}
		else
		{
			if (!plexer.IsGrounded())
			{
				goto IL_00b9;
			}
			num = destructiveContacts.Count == 0;
		}
		if (num)
		{
			goto IL_00b9;
		}
		int num2 = 0;
		goto IL_00d8;
		IL_00b9:
		num2 = ((touchingWaterOkay || waterSources.Count <= 0) ? 1 : 0);
		goto IL_00d8;
		IL_00d8:
		bool flag = touchingAshOkay && ashSources.Count > 0;
		bool flag2 = touchingWaterOkay && waterSources.Count > 0;
		bool flag3 = ((uint)num2 | (flag ? 1u : 0u) | (flag2 ? 1u : 0u)) == 0;
		if (double.IsPositiveInfinity(destroyAt) && flag3)
		{
			if (hoursOfContactAllowed <= 0f)
			{
				StartCoroutine(DestroyAndWaterAtEndOfFrame());
			}
			else
			{
				destroyAt = timeDir.HoursFromNowOrStart(hoursOfContactAllowed);
			}
		}
		else if (!double.IsPositiveInfinity(destroyAt) && !flag3)
		{
			destroyAt = double.PositiveInfinity;
		}
	}

	private IEnumerator DestroyAndWaterAtEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		DestroyAndWater();
	}

	public float PctTimeToDestruct()
	{
		return Mathf.Clamp01((float)((destroyAt - timeDir.WorldTime()) / (double)(3600f * hoursOfContactAllowed)));
	}

	private bool IsDestructiveContact(Collision col)
	{
		if (touchingWaterOkay && !IsNonWater(col))
		{
			return false;
		}
		if (touchingAshOkay && !IsNonAsh(col))
		{
			return false;
		}
		return true;
	}

	private static bool IsNonWater(Collision col)
	{
		LiquidSource component = col.gameObject.GetComponent<LiquidSource>();
		Identifiable component2 = col.gameObject.GetComponent<Identifiable>();
		bool num = component != null && Identifiable.IsWater(component.liquidId);
		bool flag = component2 != null && (component2.id == Identifiable.Id.PUDDLE_PLORT || component2.id == Identifiable.Id.PUDDLE_SLIME || Identifiable.IsWater(component2.id));
		if (!num)
		{
			return !flag;
		}
		return false;
	}

	private static bool IsNonAsh(Collision col)
	{
		AshSource component = col.gameObject.GetComponent<AshSource>();
		Identifiable component2 = col.gameObject.GetComponent<Identifiable>();
		bool num = component != null;
		bool flag = component2 != null && (component2.id == Identifiable.Id.FIRE_PLORT || component2.id == Identifiable.Id.FIRE_SLIME);
		if (!num)
		{
			return !flag;
		}
		return false;
	}
}
