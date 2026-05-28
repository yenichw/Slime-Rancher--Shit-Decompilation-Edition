using System.Collections.Generic;
using UnityEngine;

public class ExchangeAcceptor : SRBehaviour, VacShootAccelerator
{
	public GameObject storeFX;

	public ExchangeDirector.OfferType[] offerTypes;

	private ExchangeDirector.Awarder[] awarders;

	private ExchangeDirector exchangeDir;

	private SECTR_AudioSource acceptAudio;

	private HashSet<GameObject> acceptedThisFrame = new HashSet<GameObject>();

	private VacAccelerationHelper accelerationInput = VacAccelerationHelper.CreateInput();

	public void Awake()
	{
		exchangeDir = SRSingleton<SceneContext>.Instance.ExchangeDirector;
		acceptAudio = GetComponent<SECTR_AudioSource>();
		awarders = base.transform.parent.GetComponentsInChildren<ExchangeDirector.Awarder>();
	}

	public void OnTriggerEnter(Collider col)
	{
		if (col.isTrigger)
		{
			return;
		}
		Identifiable component = col.gameObject.GetComponent<Identifiable>();
		if (component != null && !acceptedThisFrame.Contains(col.gameObject) && TryAcceptAllOfferTypes(component.id))
		{
			if (storeFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(storeFX, col.transform.position, col.transform.rotation);
				acceptAudio.Play();
			}
			acceptedThisFrame.Add(col.gameObject);
			Destroyer.DestroyActor(col.gameObject, "ExchangeAcceptor.OnTriggerEnter");
			accelerationInput.OnTriggered();
		}
	}

	private bool TryAcceptAllOfferTypes(Identifiable.Id id)
	{
		bool flag = false;
		ExchangeDirector.OfferType[] array = offerTypes;
		foreach (ExchangeDirector.OfferType type in array)
		{
			flag |= exchangeDir.TryAccept(type, id, awarders);
		}
		return flag;
	}

	public void LateUpdate()
	{
		acceptedThisFrame.Clear();
	}

	public float GetVacShootSpeedFactor()
	{
		return accelerationInput.Factor;
	}
}
