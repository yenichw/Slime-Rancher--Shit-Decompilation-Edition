using System;
using UnityEngine;

public class ActivateOnProgressRange : MonoBehaviour
{
	public ProgressDirector.ProgressType progressType = ProgressDirector.ProgressType.CORPORATE_PARTNER;

	public int minProgress = int.MinValue;

	public int maxProgress = int.MaxValue;

	private ProgressDirector progressDir;

	public void Start()
	{
		progressDir = SRSingleton<SceneContext>.Instance.ProgressDirector;
		ProgressDirector progressDirector = progressDir;
		progressDirector.onProgressChanged = (ProgressDirector.OnProgressChanged)Delegate.Combine(progressDirector.onProgressChanged, new ProgressDirector.OnProgressChanged(OnProgressChanged));
		OnProgressChanged();
	}

	public void OnDestroy()
	{
		if (progressDir != null)
		{
			ProgressDirector progressDirector = progressDir;
			progressDirector.onProgressChanged = (ProgressDirector.OnProgressChanged)Delegate.Remove(progressDirector.onProgressChanged, new ProgressDirector.OnProgressChanged(OnProgressChanged));
		}
	}

	private void OnProgressChanged()
	{
		int progress = progressDir.GetProgress(progressType);
		base.gameObject.SetActive(minProgress <= progress && progress <= maxProgress);
	}
}
