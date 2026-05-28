using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class ChimeChangerActivator : SRBehaviour, TechActivator
{
	public ChimeSongList chimeSongList;

	private List<List<List<int>>> clipIndexes;

	private Animator buttonAnimator;

	private int buttonAnimation;

	private const float TIME_BETWEEN_ACTIVATIONS = 0.4f;

	private float nextActivationTime;

	private Coroutine currentCoroutine;

	private int lastClip = -1;

	private Regex clipRegex;

	public void Awake()
	{
		buttonAnimator = GetComponentInParent<Animator>();
		buttonAnimation = Animator.StringToHash("ButtonPressed");
		clipRegex = new Regex("([0-9]+)");
		clipIndexes = chimeSongList.Select((EchoNoteClusterMetadata jingle) => jingle.clips.Select((string clip) => ParseClips(clip).ToList()).ToList()).ToList();
	}

	public void Activate()
	{
		if (nextActivationTime < Time.time)
		{
			nextActivationTime = Time.time + 0.4f;
			buttonAnimator.SetTrigger(buttonAnimation);
			if (currentCoroutine != null)
			{
				StopCoroutine(currentCoroutine);
			}
			SRSingleton<GameContext>.Instance.MusicDirector.SetWigglyMode();
			SRSingleton<SceneContext>.Instance.InstrumentDirector.SelectNextInstrument();
			AnalyticsUtil.CustomEvent("ChimeChangerActivated", new Dictionary<string, object> { 
			{
				"NewInstrument",
				SRSingleton<SceneContext>.Instance.InstrumentDirector.currentInstrument.xlateKey
			} });
			int num = Randoms.SHARED.GetInt(chimeSongList.Count);
			if (num == lastClip)
			{
				num = (num + 1) % chimeSongList.Count;
			}
			currentCoroutine = StartCoroutine(PlayClips(num));
			lastClip = num;
		}
	}

	private IEnumerator PlayClips(int clipIndex)
	{
		foreach (List<int> item in clipIndexes[clipIndex])
		{
			foreach (int item2 in item)
			{
				SECTR_AudioSystem.Play(SRSingleton<SceneContext>.Instance.InstrumentDirector.currentInstrument.cue, null, base.transform.position, loop: false, item2 - 1);
			}
			yield return new WaitForSeconds(chimeSongList[clipIndex].distance / 10f);
		}
	}

	public GameObject GetCustomGuiPrefab()
	{
		return null;
	}

	private IEnumerable<int> ParseClips(string input)
	{
		foreach (Match item in clipRegex.Matches(input))
		{
			yield return int.Parse(item.Value);
		}
	}
}
