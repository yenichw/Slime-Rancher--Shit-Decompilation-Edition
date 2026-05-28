using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
	public Image bouncySlime;

	public TMP_Text tipText;

	public Sprite[] bouncyIcons;

	public GameObject autoSavePanel;

	[Tooltip("List of GameObjects to deactivate during a loading error.")]
	public List<GameObject> deactivateOnLoadError;

	[NonSerialized]
	public bool isReturningToMenu;

	private DisableDuringLoading[] toDisable;

	public void Awake()
	{
		MessageBundle bundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui");
		int num = -1;
		while (true)
		{
			int num2 = num + 1;
			if (!bundle.Exists("m.loadingtip." + num2))
			{
				break;
			}
			num = num2;
		}
		if (num >= 0)
		{
			int @int = Randoms.SHARED.GetInt(num + 1);
			tipText.text = bundle.Get("m.loadingtip." + @int);
		}
		if (bouncyIcons != null && bouncyIcons.Length != 0)
		{
			bouncySlime.sprite = Randoms.SHARED.Pick(bouncyIcons);
		}
	}

	public void OnEnable()
	{
		toDisable = UnityEngine.Object.FindObjectsOfType<DisableDuringLoading>();
		DisableDuringLoading[] array = toDisable;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
	}

	public void OnDisable()
	{
		if (!isReturningToMenu)
		{
			return;
		}
		DisableDuringLoading[] array = toDisable;
		foreach (DisableDuringLoading disableDuringLoading in array)
		{
			if (disableDuringLoading != null && disableDuringLoading.gameObject != null)
			{
				disableDuringLoading.gameObject.SetActive(value: true);
			}
		}
	}

	public void OnLoadingError()
	{
		deactivateOnLoadError.ForEach(delegate(GameObject go)
		{
			go.SetActive(value: false);
		});
	}

	public void OnLoadingStart()
	{
		deactivateOnLoadError.ForEach(delegate(GameObject go)
		{
			go.SetActive(value: true);
		});
	}
}
