using System.Collections;
using System.IO;
using DG.Tweening;
using UnityEngine;

public class CreditsUI : BaseUI
{
	public delegate void OnCreditsEndedEvent();

	public bool skippable;

	public bool doPreCredits;

	public CanvasGroup root;

	public CanvasGroup background;

	public CanvasGroup preCreditsLine1;

	public CanvasGroup preCreditsLine2;

	public CanvasGroup preCreditsLine3;

	public const float fadeTime = 1f;

	public const float fadeTimeMargin = 0.25f;

	public const float preCreditsLineFadeTime = 1f;

	public const float preCreditsLineTime = 5f;

	public const float creditsLifetime = 169f;

	private bool endReached;

	private CameraDisabler camDisabler;

	public OnCreditsEndedEvent OnCreditsEnded;

	private const float DEFAULT_CREDITS_MASTER_VOLUME = 0.1f;

	private const float DEFAULT_CREDITS_MUSIC_VOLUME = 0.1f;

	private float preCreditsMasterVolume;

	private float preCreditsMusicVolume;

	private const string MUSIC_BUS_NAME = "Music";

	private const string MASTER_BUS_NAME = "MasterBus";

	private MusicDirector musicDirector;

	public void OnEnable()
	{
		musicDirector = SRSingleton<GameContext>.Instance.MusicDirector;
		musicDirector.RegisterSuppressor(this);
		if (SRSingleton<SceneContext>.Instance.Player != null)
		{
			camDisabler = SRSingleton<SceneContext>.Instance.Player.GetComponentInChildren<CameraDisabler>();
		}
		else if (Camera.main != null)
		{
			camDisabler = Camera.main.GetComponent<CameraDisabler>();
		}
		preCreditsMasterVolume = SECTR_AudioSystem.GetBusVolume("MasterBus");
		preCreditsMusicVolume = SECTR_AudioSystem.GetBusVolume("Music");
		SECTR_AudioSystem.PauseNonUISFX(pause: true);
		StartCoroutine(DoSequence());
	}

	public void OnDisable()
	{
		if (musicDirector != null)
		{
			musicDirector.SetCreditsMode(enabled: false);
			musicDirector.DeregisterSuppressor(this);
			musicDirector.ForceStopCurrent();
			musicDirector = null;
		}
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.ProgressDirector.NoteReturnedToRanch();
		}
		SECTR_AudioSystem.SetBusVolume("MasterBus", preCreditsMasterVolume);
		SECTR_AudioSystem.SetBusVolume("Music", preCreditsMusicVolume);
		SECTR_AudioSystem.PauseNonUISFX(pause: false);
		if (camDisabler != null)
		{
			camDisabler.RemoveBlocker(this);
		}
	}

	public IEnumerator DoSequence()
	{
		if (doPreCredits)
		{
			if (camDisabler != null)
			{
				camDisabler.AddBlocker(this);
			}
			preCreditsLine1.DOFade(1f, 1f).SetUpdate(isIndependentUpdate: true);
			yield return new WaitForSecondsRealtime(5f);
			preCreditsLine2.DOFade(1f, 1f).SetUpdate(isIndependentUpdate: true);
			yield return new WaitForSecondsRealtime(5f);
			preCreditsLine3.DOFade(1f, 1f).SetUpdate(isIndependentUpdate: true);
			yield return new WaitForSecondsRealtime(5f);
			yield return new WaitForEndOfFrame();
		}
		else
		{
			background.DOFade(1f, 1f).From(0f).SetUpdate(isIndependentUpdate: true);
			yield return new WaitForSecondsRealtime(1.25f);
			yield return new WaitForEndOfFrame();
			if (camDisabler != null)
			{
				camDisabler.AddBlocker(this);
			}
		}
		SetCreditsVolume();
		musicDirector.DeregisterSuppressor(this);
		musicDirector.SetCreditsMode(enabled: true);
		GameObject scroller = GetCreditsScroll();
		yield return new WaitForSecondsRealtime(0.25f);
		scroller.GetComponent<Animator>().SetBool("ReadyToRun", value: true);
		if (doPreCredits)
		{
			preCreditsLine1.DOFade(0f, 1f).SetUpdate(isIndependentUpdate: true);
			preCreditsLine2.DOFade(0f, 1f).SetUpdate(isIndependentUpdate: true);
			preCreditsLine3.DOFade(0f, 1f).SetUpdate(isIndependentUpdate: true);
		}
		yield return new WaitForSecondsRealtime(169f);
		if (camDisabler != null)
		{
			camDisabler.RemoveBlocker(this);
		}
		root.DOFade(0f, 1f).SetUpdate(isIndependentUpdate: true);
		yield return new WaitForSecondsRealtime(1f);
		endReached = true;
		if (OnCreditsEnded != null)
		{
			OnCreditsEnded();
		}
		Close();
	}

	protected override bool Closeable()
	{
		if (base.Closeable())
		{
			if (!skippable)
			{
				return endReached;
			}
			return true;
		}
		return false;
	}

	private GameObject GetCreditsScroll()
	{
		GameObject obj = CreateCreditsScrollPrefab();
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		return obj;
	}

	private void SetCreditsVolume()
	{
		if (preCreditsMasterVolume <= 0f)
		{
			SECTR_AudioSystem.SetBusVolume("MasterBus", 0.1f);
		}
		if (preCreditsMusicVolume <= 0f)
		{
			SECTR_AudioSystem.SetBusVolume("Music", 0.1f);
		}
	}

	private GameObject CreateCreditsScrollPrefab()
	{
		string path = Path.Combine(Application.streamingAssetsPath, "credits");
		AssetBundle creditsBundle = AssetBundle.LoadFromFile(path);
		if (creditsBundle == null)
		{
			Debug.Log("Failed to load AssetBundle!: " + Application.streamingAssetsPath);
			return null;
		}
		GameObject result = Object.Instantiate(creditsBundle.LoadAsset<GameObject>("Credit_screen"));
		onDestroy = delegate
		{
			if (this != null && base.gameObject != null)
			{
				creditsBundle.Unload(unloadAllLoadedObjects: true);
			}
		};
		return result;
	}
}
