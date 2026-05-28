using System;
using System.Collections;
using InControl;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class BindingLineUI : MonoBehaviour
{
	public delegate bool DisableDelegate();

	public DisableDelegate disableDelegate;

	public PlayerAction action;

	public Button leftBtn;

	public Button rightBtn;

	public SRInput.ButtonType leftBtnMode;

	public SRInput.ButtonType rightBtnMode = SRInput.ButtonType.SECONDARY;

	public MessageBundle uiBundle;

	public MessageBundle keysBundle;

	private SRInput.ButtonType? mode;

	private OptionsUI ui;

	private bool isCurrentlyBinding;

	public void Start()
	{
		uiBundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui");
		ui = GetComponentInParent<OptionsUI>();
		Refresh();
	}

	public void BindPrimary()
	{
		Bind(leftBtnMode);
	}

	public void BindSecondary()
	{
		Bind(rightBtnMode);
	}

	public void Bind(SRInput.ButtonType btnMode)
	{
		if (!isCurrentlyBinding)
		{
			BindingSource binding = SRInput.GetBinding(action, btnMode);
			if (binding != null)
			{
				action.ListenForBindingReplacing(binding);
			}
			else
			{
				action.ListenForBinding();
			}
			EnterBindingState(btnMode);
			SetMode(btnMode);
			SetButtonText(uiBundle.Get("m.press_key"));
		}
	}

	private void OnBindingRejected(PlayerAction arg1, BindingSource arg2, BindingSourceRejectionType arg3)
	{
		StartCoroutine(ResetBindingState());
	}

	private void OnBindingAdded(PlayerAction arg1, BindingSource arg2)
	{
		StartCoroutine(ResetBindingState());
	}

	private void EnterBindingState(SRInput.ButtonType btnMode)
	{
		BindingListenOptions listenOptions = SRInput.Actions.ListenOptions;
		listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(listenOptions.OnBindingAdded, new Action<PlayerAction, BindingSource>(OnBindingAdded));
		BindingListenOptions listenOptions2 = SRInput.Actions.ListenOptions;
		listenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Combine(listenOptions2.OnBindingRejected, new Action<PlayerAction, BindingSource, BindingSourceRejectionType>(OnBindingRejected));
		if (btnMode == SRInput.ButtonType.GAMEPAD || btnMode == SRInput.ButtonType.GAMEPAD_SEC)
		{
			BindingListenOptions listenOptions3 = SRInput.Actions.ListenOptions;
			listenOptions3.OnBindingFound = (Func<PlayerAction, BindingSource, bool>)Delegate.Combine(listenOptions3.OnBindingFound, new Func<PlayerAction, BindingSource, bool>(IsGamepadBinding));
		}
		else
		{
			BindingListenOptions listenOptions4 = SRInput.Actions.ListenOptions;
			listenOptions4.OnBindingFound = (Func<PlayerAction, BindingSource, bool>)Delegate.Combine(listenOptions4.OnBindingFound, new Func<PlayerAction, BindingSource, bool>(IsKeyboardMouseBinding));
		}
		isCurrentlyBinding = true;
		SelectImageForAction componentInChildren = base.gameObject.GetComponentInChildren<SelectImageForAction>();
		if (componentInChildren != null)
		{
			componentInChildren.gameObject.SetActive(value: false);
		}
	}

	private bool IsGamepadBinding(PlayerAction action, BindingSource binding)
	{
		if (binding.BindingSourceType != BindingSourceType.KeyBindingSource)
		{
			return binding.BindingSourceType != BindingSourceType.MouseBindingSource;
		}
		return false;
	}

	private bool IsKeyboardMouseBinding(PlayerAction action, BindingSource binding)
	{
		if (binding.BindingSourceType != BindingSourceType.KeyBindingSource)
		{
			return binding.BindingSourceType == BindingSourceType.MouseBindingSource;
		}
		return true;
	}

	private IEnumerator ResetBindingState()
	{
		BindingListenOptions listenOptions = SRInput.Actions.ListenOptions;
		listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Remove(listenOptions.OnBindingAdded, new Action<PlayerAction, BindingSource>(OnBindingAdded));
		BindingListenOptions listenOptions2 = SRInput.Actions.ListenOptions;
		listenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Remove(listenOptions2.OnBindingRejected, new Action<PlayerAction, BindingSource, BindingSourceRejectionType>(OnBindingRejected));
		yield return new WaitForEndOfFrame();
		isCurrentlyBinding = false;
		SelectImageForAction componentInChildren = base.gameObject.GetComponentInChildren<SelectImageForAction>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.gameObject.SetActive(value: true);
		}
	}

	public IEnumerator Delay(UnityAction action)
	{
		yield return new WaitForEndOfFrame();
		action();
	}

	public void Refresh()
	{
		SetButtonText(leftBtn, XlateKeyText.XlateKey(GetCurrKey(leftBtnMode)));
		if (rightBtn != null)
		{
			SetButtonText(rightBtn, XlateKeyText.XlateKey(GetCurrKey(rightBtnMode)));
		}
		if (mode.HasValue)
		{
			StartCoroutine(DelayedResetMode());
		}
	}

	public IEnumerator DelayedResetMode()
	{
		yield return new WaitForEndOfFrame();
		action.StopListeningForBinding();
		SetMode(null);
	}

	private string GetCurrKey(SRInput.ButtonType mode)
	{
		return SRInput.GetButtonKey(action, mode) ?? Key.None.ToString();
	}

	private void SetButtonText(string text)
	{
		SetButtonText((mode == leftBtnMode) ? leftBtn : rightBtn, text);
	}

	private static void SetButtonText(Button btn, string text)
	{
		TMP_Text[] componentsInChildren = btn.GetComponentsInChildren<TMP_Text>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].text = text;
		}
	}

	private void SetMode(SRInput.ButtonType? mode)
	{
		this.mode = mode;
		bool hasValue = mode.HasValue;
		ui.PreventClosing(hasValue);
		TabByMenuKeys.disabledForBinding = hasValue;
		EventSystem.current.sendNavigationEvents = !hasValue;
		((SRStandaloneInputModule)EventSystem.current.currentInputModule).processMouseEvents = !hasValue;
	}
}
