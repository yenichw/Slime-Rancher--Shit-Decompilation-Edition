using System;
using System.Collections;
using Assets.Script.Util.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RancherChatUI : BaseUI
{
	[Tooltip("Rancher image.")]
	public Image rancherImage;

	[Tooltip("Text showing the rancher's name.")]
	public TMP_Text rancherName;

	[Tooltip("Message background image.")]
	public Image messageBackground;

	[Tooltip("Message text.")]
	public TMP_Text messageText;

	[Tooltip("Message prefab parent.")]
	public Transform messagePrefabParent;

	[Tooltip("Image showing the button prompt.")]
	public Image promptImage;

	[Tooltip("Left mouse sprite to show on the button prompt when a mouse is active.")]
	public Sprite promptImageLeftMouse;

	private const string ANIMATION_READY = "rancherChat_loop";

	private static readonly int ANIMATION_NEXT = Animator.StringToHash("next");

	private const float ANIMATION_NEXT_WAIT = 0.1f;

	private static readonly int ANIMATION_EXIT = Animator.StringToHash("exit");

	private const float ANIMATION_EXIT_WAIT = 0.5f;

	private InputDirector inputDirector;

	private MessageBundle bundle;

	private Animator animator;

	private RancherChatMetadata metadata;

	private int index;

	public static RancherChatUI Instantiate(RancherChatMetadata metadata)
	{
		RancherChatUI requiredComponent = UnityEngine.Object.Instantiate(SRSingleton<GameContext>.Instance.UITemplates.rancherChatPrefab).GetRequiredComponent<RancherChatUI>();
		requiredComponent.metadata = metadata;
		requiredComponent.Refresh();
		return requiredComponent;
	}

	public override void Awake()
	{
		base.Awake();
		animator = GetRequiredComponent<Animator>();
		inputDirector = SRSingleton<GameContext>.Instance.InputDirector;
		InputDirector obj = inputDirector;
		obj.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Combine(obj.onKeysChanged, new InputDirector.OnKeysChanged(OnKeysChanged));
		OnKeysChanged();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (inputDirector != null)
		{
			InputDirector obj = inputDirector;
			obj.onKeysChanged = (InputDirector.OnKeysChanged)Delegate.Remove(obj.onKeysChanged, new InputDirector.OnKeysChanged(OnKeysChanged));
			inputDirector = null;
		}
	}

	public override void OnBundlesAvailable(MessageDirector messageDirector)
	{
		base.OnBundlesAvailable(messageDirector);
		bundle = messageDirector.GetBundle("exchange");
		Refresh();
	}

	protected override bool Closeable()
	{
		return false;
	}

	private void OnKeysChanged()
	{
		promptImage.sprite = (InputDirector.UsingGamepad() ? inputDirector.GetActiveDeviceIcon("Submit", isPauseAction: true, out var _) : promptImageLeftMouse);
	}

	public void OnButtonPressed_Next()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("rancherChat_loop"))
		{
			StartCoroutine(OnButtonPressed_Next_Coroutine());
		}
	}

	private IEnumerator OnButtonPressed_Next_Coroutine()
	{
		index++;
		if (index >= metadata.entries.Length)
		{
			animator.SetTrigger(ANIMATION_EXIT);
			yield return new WaitForSecondsRealtime(0.5f);
			Close();
		}
		else
		{
			animator.SetTrigger(ANIMATION_NEXT);
			yield return new WaitForSecondsRealtime(0.1f);
			Refresh();
		}
	}

	private void Refresh()
	{
		if (metadata != null && index < metadata.entries.Length)
		{
			RancherChatMetadata.Entry entry = metadata.entries[index];
			rancherName.text = bundle.Get($"m.rancher.{entry.rancherName.ToString().ToLowerInvariant()}");
			rancherImage.sprite = entry.rancherImage;
			if (entry.messagePrefab != null)
			{
				messageText.gameObject.SetActive(value: false);
				messagePrefabParent.gameObject.SetActive(value: true);
				messagePrefabParent.DestroyChildren("RancherChatUI.Refresh");
				UnityEngine.Object.Instantiate(entry.messagePrefab, messagePrefabParent);
			}
			else
			{
				messageText.gameObject.SetActive(value: true);
				messagePrefabParent.gameObject.SetActive(value: false);
				messagePrefabParent.DestroyChildren("RancherChatUI.Refresh");
				messageText.text = bundle.Get(entry.messageText);
			}
			messageBackground.material = entry.messageBackground;
		}
	}
}
