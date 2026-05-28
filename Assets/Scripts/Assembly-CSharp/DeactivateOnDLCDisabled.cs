using DLCPackage;
using UnityEngine;

public class DeactivateOnDLCDisabled : MonoBehaviour
{
	public Id requiredDlc;

	private DLCDirector director;

	public void Start()
	{
		director = SRSingleton<GameContext>.Instance.DLCDirector;
		director.onPackageInstalled += CheckDLCState;
		CheckDLCState(requiredDlc);
	}

	public void OnDestroy()
	{
		if (director != null)
		{
			director.onPackageInstalled -= CheckDLCState;
			director = null;
		}
	}

	private void CheckDLCState(Id package)
	{
		if (package == requiredDlc)
		{
			base.gameObject.SetActive(director.IsPackageInstalledAndEnabled(requiredDlc));
		}
	}
}
