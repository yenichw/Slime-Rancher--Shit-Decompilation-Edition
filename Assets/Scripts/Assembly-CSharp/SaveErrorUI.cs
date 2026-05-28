using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveErrorUI : BaseUI
{
	private enum ErrorType
	{
		NONE = 0,
		LOAD = 1,
		SAVE = 2
	}

	private ErrorType errorType;

	public GameObject retryButton;

	public void SetException(Exception e, string path)
	{
		if (e is UnauthorizedAccessException)
		{
			Error(MessageUtil.Tcompose("e.savefile_inaccessible", Path.GetFullPath(path)), neverClear: true);
		}
		else if (e is ArgumentException || e is ArgumentNullException || e is NotSupportedException || e is PathTooLongException)
		{
			Error(MessageUtil.Tcompose("e.savefile_invalid_name", Path.GetFullPath(path)), neverClear: true);
		}
		else if (e is DirectoryNotFoundException)
		{
			Error(MessageUtil.Tcompose("e.savefile_dir_not_found", Path.GetFullPath(path)), neverClear: true);
		}
		else
		{
			Error(MessageUtil.Tcompose("e.savefile_unknown", Path.GetFullPath(path)), neverClear: true);
		}
		errorType = ErrorType.SAVE;
		retryButton.gameObject.SetActive(value: true);
	}

	public void SetLoadException(Exception e, string path)
	{
		Error(MessageUtil.Tcompose("e.pushfile_error", path), neverClear: true);
		errorType = ErrorType.LOAD;
		retryButton.gameObject.SetActive(value: false);
	}

	public void RetrySave()
	{
		if (errorType == ErrorType.SAVE)
		{
			Close();
			SRSingleton<GameContext>.Instance.AutoSaveDirector.SaveAllNow();
		}
	}

	protected override bool Closeable()
	{
		return false;
	}

	public void Quit()
	{
		Close();
		SceneManager.LoadScene("MainMenu");
	}
}
