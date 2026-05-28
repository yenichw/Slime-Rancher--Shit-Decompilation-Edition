using System.Collections;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class Nutcracker : SRBehaviour
{
	public Identifiable.Id convertFromId = Identifiable.Id.KOOKADOBA_BALL;

	public Identifiable.Id convertToId = Identifiable.Id.KOOKADOBA_FRUIT;

	public int convertToCount = 5;

	public GameObject crackFX;

	public SECTR_AudioCue crackCue;

	private LookupDirector lookupDir;

	private Animator anim;

	private Region region;

	private int activateId;

	private const float SPAWN_RAD = 0.5f;

	private const float TIME_BEFORE_CREATE = 3f;

	public void Awake()
	{
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		region = GetComponentInParent<Region>();
		anim = GetComponentInParent<Animator>();
		activateId = Animator.StringToHash("activate");
	}

	public void OnTriggerEnter(Collider col)
	{
		if (!col.isTrigger && Identifiable.GetId(col.gameObject) == convertFromId)
		{
			StartCoroutine(DoCrack(col.gameObject));
		}
	}

	private IEnumerator DoCrack(GameObject toCrack)
	{
		toCrack.GetComponent<Rigidbody>().isKinematic = true;
		toCrack.GetComponent<Collider>().enabled = false;
		Renderer[] componentsInChildren = toCrack.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		anim.SetTrigger(activateId);
		if (crackCue != null)
		{
			SECTR_AudioSystem.Play(crackCue, base.transform.position, loop: false);
		}
		yield return new WaitForSeconds(3f);
		GameObject prefab = lookupDir.GetPrefab(convertToId);
		if (crackFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(crackFX, toCrack.transform.position, toCrack.transform.rotation);
		}
		for (int j = 0; j < convertToCount; j++)
		{
			Vector3 position = base.transform.position + Random.insideUnitSphere * 0.5f;
			SRBehaviour.InstantiateActor(prefab, region.setId, position, Quaternion.LookRotation(Random.onUnitSphere));
		}
		Destroyer.DestroyActor(toCrack, "Nutcracker.DoCrack");
	}
}
