using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class EchoNoteGordo : IdHandler, EchoNoteGordoModel.Participant
{
	[Tooltip("Parent GameObject containing the gordo model.")]
	public GameObject gordo;

	[Tooltip("SFX played when the EchoNoteGordo is active.")]
	public SECTR_AudioCue onActiveCue;

	private SECTR_AudioCueInstance onActiveCueInstance;

	[Tooltip("SFX played when the EchoNoteGordo is popping.")]
	public SECTR_AudioCue onPoppingCue;

	private SECTR_AudioCueInstance onPoppingCueInstance;

	[Tooltip("Parent GameObject containing the portal ring.")]
	public GameObject ring;

	[Tooltip("Instruments unlocked in addition to the next instrument in the unlock sequence.")]
	public InstrumentModel.Instrument[] bonusInstruments;

	private const float POPPING_DISTANCE = 8f;

	private const float POPPING_DISTANCE_SQR = 64f;

	private static readonly int PROPERTY_FADE = Shader.PropertyToID("_Fade");

	private EchoNoteGordoModel model;

	private Animator animator;

	private TeleportSource teleporter;

	public void Awake()
	{
		animator = GetComponentInChildren<Animator>();
		teleporter = GetComponentInChildren<TeleportSource>();
		SRSingleton<SceneContext>.Instance.GameModel.RegisterEchoNoteGordo(base.id, base.gameObject);
	}

	public void Start()
	{
	}

	public void OnEnable()
	{
		if (model == null || !SRSingleton<SceneContext>.Instance.GameModel.GetHolidayModel().eventEchoNoteGordos.Any((HolidayModel.EventEchoNoteGordo e) => e.objectId == base.id))
		{
			teleporter.waitForExternalActivation = true;
			base.gameObject.SetActive(value: false);
			return;
		}
		if (model.state == EchoNoteGordoModel.State.NOT_POPPED)
		{
			onActiveCueInstance.Stop(stopImmediately: true);
			onActiveCueInstance = SECTR_AudioSystem.Play(onActiveCue, gordo.transform.position, loop: true);
		}
		onPoppingCueInstance.Pause(paused: false);
		teleporter.waitForExternalActivation = model.state != EchoNoteGordoModel.State.POPPED;
		gordo.SetActive(model.state != EchoNoteGordoModel.State.POPPED);
		ring.SetActive(model.state == EchoNoteGordoModel.State.POPPED);
	}

	public void OnDisable()
	{
		onActiveCueInstance.Stop(stopImmediately: true);
		onPoppingCueInstance.Pause(paused: true);
	}

	public void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.GameModel.UnregisterEchoNoteGordo(base.id);
		}
		onPoppingCueInstance.Stop(stopImmediately: true);
	}

	public void Update()
	{
		if (model.state == EchoNoteGordoModel.State.NOT_POPPED && (SRSingleton<SceneContext>.Instance.Player.transform.position - gordo.transform.position).sqrMagnitude <= 64f)
		{
			SRSingleton<SceneContext>.Instance.PediaDirector.MaybeShowPopup(PediaDirector.Id.ECHO_NOTE_GORDO_SLIME);
			model.state = EchoNoteGordoModel.State.POPPING_1;
			animator.SetBool("ACTIVATED", value: true);
		}
	}

	protected override string IdPrefix()
	{
		return "gordoEchoNote";
	}

	public void InitModel(EchoNoteGordoModel model)
	{
		model.state = EchoNoteGordoModel.State.NOT_POPPED;
	}

	public void SetModel(EchoNoteGordoModel model)
	{
		this.model = model;
		if (this.model.state < EchoNoteGordoModel.State.POPPED)
		{
			this.model.state = EchoNoteGordoModel.State.NOT_POPPED;
		}
	}

	public void OnAnimationEvent_StateEnter(EchoNoteGordoAnimatorState.Id id)
	{
		if (id == EchoNoteGordoAnimatorState.Id.ACTIVATION && model.state == EchoNoteGordoModel.State.POPPING_1)
		{
			model.state = EchoNoteGordoModel.State.POPPING_2;
			onActiveCueInstance.Stop(stopImmediately: false);
			onPoppingCueInstance.Stop(stopImmediately: true);
			onPoppingCueInstance = SECTR_AudioSystem.Play(onPoppingCue, gordo.transform.position, loop: false);
		}
	}

	public void OnAnimationEvent_StateExit(EchoNoteGordoAnimatorState.Id id)
	{
		if (id == EchoNoteGordoAnimatorState.Id.ACTIVATION && model.state == EchoNoteGordoModel.State.POPPED)
		{
			teleporter.waitForExternalActivation = false;
			gordo.SetActive(value: false);
			ring.SetActive(value: true);
		}
	}

	public void OnAnimationEvent_Popped()
	{
		if (model.state != EchoNoteGordoModel.State.POPPING_2)
		{
			return;
		}
		model.state = EchoNoteGordoModel.State.POPPED;
		AnalyticsUtil.CustomEvent("TwinkleSlimeBurst", new Dictionary<string, object>
		{
			{ "type", base.name },
			{ "twinkleId", base.id }
		});
		RegionRegistry.RegionSetId setId = GetComponentInParent<Region>().setId;
		SRSingleton<SceneContext>.Instance.InstrumentDirector.UnlockNextInstrument();
		InstrumentModel.Instrument[] array = bonusInstruments;
		foreach (InstrumentModel.Instrument instrument in array)
		{
			SRSingleton<SceneContext>.Instance.InstrumentDirector.UnlockInstrument(instrument);
		}
		EchoNoteMetadata[] componentsInChildren = GetComponentsInChildren<EchoNoteMetadata>(includeInactive: true);
		foreach (EchoNoteMetadata echoNoteMetadata in componentsInChildren)
		{
			GameObject gameObject = SRBehaviour.InstantiateActor(SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(echoNoteMetadata.id), setId, echoNoteMetadata.transform.position, echoNoteMetadata.transform.rotation);
			float inRange = Randoms.SHARED.GetInRange(0f, 1f);
			Renderer chimeRenderer = gameObject.GetComponentInChildren<Renderer>();
			chimeRenderer.gameObject.AddComponent<PauseTweenOnDisable>().tween = DOTween.To(() => GetMaterialFade(chimeRenderer), delegate(float fade)
			{
				SetMaterialFade(chimeRenderer, fade);
			}, 1f, 3f).From(0f).SetDelay(inRange);
		}
		teleporter.waitForExternalActivation = false;
		ring.SetActive(value: true);
	}

	private float GetMaterialFade(Renderer targetRenderer)
	{
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		targetRenderer.GetPropertyBlock(materialPropertyBlock);
		return materialPropertyBlock.GetFloat(PROPERTY_FADE);
	}

	private void SetMaterialFade(Renderer targetRenderer, float fade)
	{
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		targetRenderer.GetPropertyBlock(materialPropertyBlock);
		materialPropertyBlock.SetFloat(PROPERTY_FADE, fade);
		targetRenderer.SetPropertyBlock(materialPropertyBlock);
	}

	public void PrepareWorldGenerated()
	{
		GameObject gameObject = new GameObject("cluster_notes_metadata");
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			EchoNote[] componentsInChildren = base.transform.GetChild(num).GetComponentsInChildren<EchoNote>(includeInactive: true);
			if (componentsInChildren.Length != 0)
			{
				EchoNote[] array = componentsInChildren;
				foreach (EchoNote echoNote in array)
				{
					GameObject obj = new GameObject(string.Format("echoNote{0}", echoNote.clip.ToString("D2")));
					obj.transform.SetParent(gameObject.transform, worldPositionStays: false);
					obj.transform.position = echoNote.transform.position;
					obj.AddComponent<EchoNoteMetadata>().id = (Identifiable.Id)(17000 + echoNote.clip - 1);
				}
				if (Application.isPlaying)
				{
					Destroyer.Destroy(base.transform.GetChild(num).gameObject, 0f, "EchoNoteGordo.Start", asActorOk: true);
				}
				else
				{
					Object.DestroyImmediate(base.transform.GetChild(num).gameObject);
				}
			}
		}
		_ = Application.isPlaying;
	}

	public void OnDrawGizmosSelected()
	{
		if (gordo != null)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(gordo.transform.position, 8f);
		}
	}
}
