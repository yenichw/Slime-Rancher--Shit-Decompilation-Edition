using System;
using System.Linq;
using DLCPackage;
using TMPro;
using UnityEngine;

public class DLCPurgedExceptionUI : BaseUI
{
	[Tooltip("Text showing the error message.")]
	public TMP_Text message;

	private DLCPurgedException exception;

	private Action onContinue;

	private Action onCancel;

	public static DLCPurgedExceptionUI OnExceptionCaught(DLCPurgedExceptionUI prefab, DLCPurgedException exception, Action onContinue, Action onCancel)
	{
		DLCPurgedExceptionUI dLCPurgedExceptionUI = UnityEngine.Object.Instantiate(prefab);
		dLCPurgedExceptionUI.exception = exception;
		dLCPurgedExceptionUI.onContinue = onContinue;
		dLCPurgedExceptionUI.onCancel = onCancel;
		dLCPurgedExceptionUI.RebuildUI();
		return dLCPurgedExceptionUI;
	}

	public override void Update()
	{
		if (Closeable() && SRInput.PauseActions.cancel.WasPressed)
		{
			if (onCancel != null)
			{
				onCancel();
			}
			Close();
		}
	}

	public override void OnBundlesAvailable(MessageDirector messageDirector)
	{
		base.OnBundlesAvailable(messageDirector);
		RebuildUI();
	}

	private void RebuildUI()
	{
		MessageDirector messageDirector = SRSingleton<GameContext>.Instance.MessageDirector;
		MessageBundle bundle = messageDirector.GetBundle("ui");
		MessageBundle pedia = messageDirector.GetBundle("pedia");
		message.SetText((exception != null) ? bundle.Get("e.file_load_failed.dlc_purged", string.Join("\n", exception.packages.Select((Id p) => pedia.Get($"m.dlc.{p.ToString().ToLowerInvariant()}")).ToArray())) : string.Empty);
	}

	public void OnContinue()
	{
		if (onContinue != null)
		{
			onContinue();
		}
	}

	public void OnShowPackageInStore()
	{
		if (exception != null && onCancel != null)
		{
			SRSingleton<GameContext>.Instance.DLCDirector.ShowPackageInStore(exception.packages.First());
			onCancel();
		}
	}

	public void OnCancel()
	{
		if (onCancel != null)
		{
			onCancel();
		}
	}
}
