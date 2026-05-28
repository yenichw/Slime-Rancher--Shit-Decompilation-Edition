using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

[RequireComponent(typeof(GordoFaceAnimator))]
public class EventGordoEat : GordoEat
{
	private class EventGordoMusic : MonoBehaviour
	{
		private const float MAX_DISTANCE = 30f;

		private const float MAX_DISTANCE_SQR = 900f;

		private GameObject player;

		private MusicDirector music;

		private float time;

		public void Awake()
		{
			player = SRSingleton<SceneContext>.Instance.Player;
			music = SRSingleton<GameContext>.Instance.MusicDirector;
			music.SetEventGordoMode(enabled: true);
			time = Time.unscaledTime + music.eventGordoMusic.MinClipLength() - music.eventGordoMusic.FadeOutTime;
		}

		public void Update()
		{
			if (Time.unscaledTime >= time || (base.transform.position - player.transform.position).sqrMagnitude >= 900f)
			{
				Destroyer.Destroy(base.gameObject, "EventGordoMusic.Update");
			}
		}

		public void OnDestroy()
		{
			music.SetEventGordoMode(enabled: false);
		}
	}

	[Tooltip("SFX played when the EventGordo is enabled.")]
	public SECTR_AudioCue onActiveCue;

	private SECTR_AudioCueInstance onActiveCueInstance;

	public void OnEnable()
	{
		if (Application.isPlaying)
		{
			if (gordoModel == null || gordoModel.gordoEatenCount == -1 || !SRSingleton<SceneContext>.Instance.GameModel.GetHolidayModel().eventGordos.Any((HolidayModel.EventGordo e) => e.objectId == base.id))
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			onActiveCueInstance.Stop(stopImmediately: false);
			onActiveCueInstance = SECTR_AudioSystem.Play(onActiveCue, base.transform.position, loop: true);
		}
	}

	public void OnDisable()
	{
		if (Application.isPlaying)
		{
			onActiveCueInstance.Stop(stopImmediately: false);
		}
	}

	public override void SetModel(GordoModel model)
	{
		gordoModel = model;
		if (gordoModel.gordoEatenCount != -1)
		{
			base.SetModel(model);
		}
	}

	protected override PediaDirector.Id GetPediaId()
	{
		return PediaDirector.Id.PARTY_GORDO_SLIME;
	}

	protected override void DidCompleteBurst()
	{
		base.DidCompleteBurst();
		GameObject obj = new GameObject("EventGordoMusic");
		obj.transform.position = base.transform.position;
		obj.AddComponent<EventGordoMusic>();
	}
}
