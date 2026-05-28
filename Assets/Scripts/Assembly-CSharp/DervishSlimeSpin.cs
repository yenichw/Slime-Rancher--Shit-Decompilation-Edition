using System.Collections.Generic;
using UnityEngine;

public class DervishSlimeSpin : SlimeHover, LiquidConsumer
{
	[Tooltip("The mini-vortex we attach under ourselves")]
	public GameObject tornadoPrefab;

	[Tooltip("The mini-vortex we attach under ourselves")]
	public GameObject tornadoLargoPrefab;

	[Tooltip("The full self-moving whirlwind we spawn only when agitated")]
	public GameObject fullWhirlwindPrefab;

	private const float STD_HOVER_HEIGHT = 5f;

	private const float INV_STD_HOVER_HEIGHT = 0.2f;

	private const float LARGO_HOVER_HEIGHT = 9f;

	private const float INV_LARGO_HOVER_HEIGHT = 1f / 9f;

	private const float WHIRLWIND_CUTOFF = 0.95f;

	private const int MAX_WHIRLWINDS = 6;

	private GameObject tornado;

	private TotemLinker totemLinker;

	private CalmedByWaterSpray calmer;

	private SlimeAppearanceApplicator slimeAppearanceApplicator;

	private bool isLargo;

	private static Queue<GameObject> whirlwinds = new Queue<GameObject>();

	public override void Start()
	{
		base.Start();
		totemLinker = GetComponentInChildren<TotemLinker>();
		calmer = GetComponent<CalmedByWaterSpray>();
		isLargo = GetComponent<Vacuumable>().size != Vacuumable.Size.NORMAL;
		slimeAppearanceApplicator = GetComponent<SlimeAppearanceApplicator>();
		slimeAppearanceApplicator.OnAppearanceChanged += UpdateTornadoAppearance;
		if (slimeAppearanceApplicator.Appearance != null)
		{
			UpdateTornadoAppearance(slimeAppearanceApplicator.Appearance);
		}
	}

	public override float Relevancy(bool isGrounded)
	{
		if (calmer.IsCalmed())
		{
			return 0f;
		}
		return base.Relevancy(isGrounded);
	}

	public override void Selected()
	{
		base.Selected();
		if (tornado == null)
		{
			tornado = SpawnTornado();
		}
		if (totemLinker != null)
		{
			totemLinker.DisableToteming();
		}
		if (emotions.GetCurr(SlimeEmotions.Emotion.AGITATION) >= 0.95f)
		{
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			eulerAngles.x = 0f;
			eulerAngles.z = 0f;
			Quaternion rotation = Quaternion.Euler(eulerAngles);
			while (whirlwinds.Count > 0 && whirlwinds.Peek() == null)
			{
				whirlwinds.Dequeue();
			}
			if (whirlwinds.Count > 6)
			{
				whirlwinds.Dequeue().GetComponent<DestroyAfterTime>().SetDeathTime(0.0);
			}
			GameObject gameObject = SRBehaviour.InstantiateDynamic(fullWhirlwindPrefab, base.transform.position, rotation);
			gameObject.GetComponent<DestroyAfterTime>().FizzleParticlesOnDestroy();
			whirlwinds.Enqueue(gameObject);
		}
	}

	public override void Deselected()
	{
		base.Deselected();
		if (tornado != null)
		{
			Destroyer.Destroy(tornado, "DervishSlimeSpin.Deselected");
		}
		if (totemLinker != null)
		{
			totemLinker.EnableToteming();
		}
	}

	protected override float GetHoverAccel()
	{
		return 600f;
	}

	protected override float GetHoverHeight()
	{
		if (!isLargo)
		{
			return 5f;
		}
		return 9f;
	}

	protected override float GetInvHoverHeight()
	{
		if (!isLargo)
		{
			return 0.2f;
		}
		return 1f / 9f;
	}

	private void UpdateTornadoAppearance(SlimeAppearance appearance)
	{
		tornadoPrefab = appearance.TornadoAppearance.tornadoPrefab;
		tornadoLargoPrefab = appearance.TornadoAppearance.largoTornadoPrefab;
		fullWhirlwindPrefab = appearance.TornadoAppearance.fullWhirlwindPrefab;
	}

	private GameObject SpawnTornado()
	{
		GameObject obj = SRBehaviour.InstantiateDynamic(isLargo ? tornadoLargoPrefab : tornadoPrefab, base.transform.position, Quaternion.identity);
		obj.GetComponent<KeepAlignedUnderActor>().AlignWith(base.transform);
		return obj;
	}

	public void AddLiquid(Identifiable.Id liquidId, float units)
	{
		if (Identifiable.IsWater(liquidId))
		{
			plexer.ForceRethink();
		}
	}
}
