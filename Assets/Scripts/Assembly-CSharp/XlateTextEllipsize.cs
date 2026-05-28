using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XlateTextEllipsize : MonoBehaviour
{
	public string bundlePath = "ui";

	public string key;

	public string[] args;

	[Tooltip("Time between ellipsis steps (in real-world clock time).")]
	public float timePerChange = 1f;

	private MessageBundle bundle;

	private Text text;

	private TMP_Text meshText;

	private float ellipsisChangeTime;

	private int ellipsisCount;

	private string unellipsizedText;

	public void Awake()
	{
		text = GetComponent<Text>();
		meshText = GetComponent<TMP_Text>();
		SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(InitBundles);
	}

	public void OnDestroy()
	{
		if (SRSingleton<GameContext>.Instance != null && SRSingleton<GameContext>.Instance.MessageDirector != null)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.UnregisterBundlesListener(InitBundles);
		}
	}

	public void InitBundles(MessageDirector msgDir)
	{
		bundle = msgDir.GetBundle(bundlePath);
		string[] array = new string[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			array[i] = bundle.Xlate(args[i]);
		}
		unellipsizedText = bundle.Get(key, array);
		if (text != null)
		{
			text.text = unellipsizedText;
		}
		if (meshText != null)
		{
			meshText.text = unellipsizedText;
		}
	}

	public void Update()
	{
		if (Time.unscaledTime > ellipsisChangeTime)
		{
			if (text != null)
			{
				text.text = bundle.Xlate(MessageUtil.Compose("m.ellipsize" + ellipsisCount, MessageUtil.Taint(unellipsizedText)));
			}
			if (meshText != null)
			{
				meshText.text = bundle.Xlate(MessageUtil.Compose("m.ellipsize" + ellipsisCount, MessageUtil.Taint(unellipsizedText)));
			}
			ellipsisCount = (ellipsisCount + 1) % 4;
			ellipsisChangeTime = Time.unscaledTime + timePerChange;
		}
	}
}
