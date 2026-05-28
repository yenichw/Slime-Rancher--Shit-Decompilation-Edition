using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class FileStorageProvider : StorageProvider
{
	private const string EXTENSION = ".sav";

	private const string TEMP_EXTENSION = ".tmp";

	private const string PROFILE_FILENAME = "slimerancher.prf";

	private const string SETTINGS_FILENAME = "slimerancher.cfg";

	private bool isInitialized;

	public void Initialize()
	{
		Directory.CreateDirectory(SavePath());
		try
		{
			MaybeMoveOldData();
		}
		catch (Exception ex)
		{
			Log.Debug("Attempted to move old data, failed.", "Exception", ex);
		}
		isInitialized = true;
	}

	public bool IsInitialized()
	{
		return isInitialized;
	}

	private void MaybeMoveOldData()
	{
	}

	public void GetGameData(string name, MemoryStream dataStream)
	{
		string fullFilePath = GetFullFilePath(name);
		Load(fullFilePath, name, dataStream);
	}

	private void Load(string path, string name, MemoryStream loadInto)
	{
		if (File.Exists(path))
		{
			using (FileStream from = File.Open(path, FileMode.Open))
			{
				CopyStream(from, loadInto);
				return;
			}
		}
		throw new FileNotFoundException("No save file found", path);
	}

	private void CopyStream(Stream from, Stream to)
	{
		byte[] array = new byte[1024];
		int num = 0;
		do
		{
			num = from.Read(array, 0, array.Length);
			to.Write(array, 0, num);
		}
		while (num >= array.Length);
	}

	public List<string> GetAvailableGames()
	{
		string path = SavePath();
		if (!Directory.Exists(path))
		{
			return new List<string>();
		}
		return (from f in Directory.GetFiles(path, "*.sav")
			select Path.GetFileNameWithoutExtension(f)).ToList();
	}

	public string GetGameId(string name)
	{
		return string.Empty;
	}

	public void StoreGameData(string gameId, string gameName, string name, MemoryStream stream)
	{
		string fullFilePath = GetFullFilePath(name);
		string text = string.Format("{0}{1}", fullFilePath, ".tmp");
		using (FileStream to = File.Create(text))
		{
			CopyStream(stream, to);
		}
		File.Copy(text, fullFilePath, overwrite: true);
		try
		{
			File.Delete(text);
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to delete temporary save file.", "temp file", text, "Exception", ex.Message);
		}
	}

	public void DeleteGameData(string name)
	{
		string fullFilePath = GetFullFilePath(name);
		if (File.Exists(fullFilePath))
		{
			File.Delete(fullFilePath);
			return;
		}
		throw new FileNotFoundException("No file found to delete", fullFilePath);
	}

	public void DeleteGamesData(List<string> names)
	{
		foreach (string name in names)
		{
			DeleteGameData(name);
		}
	}

	public bool HasGameData(string name)
	{
		return File.Exists(GetFullFilePath(name));
	}

	private string GetFullFilePath(string name)
	{
		return Path.Combine(SavePath(), string.Format("{0}{1}", name, ".sav"));
	}

	private string SavePath()
	{
		string text = Application.persistentDataPath;
		string text2 = "unity.Monomi Park.Slime Rancher";
		if (text.EndsWith(text2))
		{
			text = text.Replace(text2, Path.Combine("Monomi Park", "Slime Rancher"));
		}
		return text;
	}

	public bool HasProfile()
	{
		return FileExists("slimerancher.prf");
	}

	public void GetProfileData(MemoryStream dataStream)
	{
		LoadDataStream("slimerancher.prf", dataStream);
	}

	public void StoreProfileData(MemoryStream profileDataStream)
	{
		StoreDataStream("slimerancher.prf", profileDataStream);
	}

	public bool HasSettings()
	{
		return FileExists("slimerancher.cfg");
	}

	public void GetSettingsData(MemoryStream dataStream)
	{
		LoadDataStream("slimerancher.cfg", dataStream);
	}

	public void StoreSettingsData(MemoryStream dataStream)
	{
		StoreDataStream("slimerancher.cfg", dataStream);
	}

	public void Flush()
	{
	}

	private void LoadDataStream(string fileName, MemoryStream stream)
	{
		string text = ToPath(fileName);
		Log.Debug("Loading data from file.", text);
		if (File.Exists(text))
		{
			using (FileStream from = File.Open(text, FileMode.Open))
			{
				CopyStream(from, stream);
				return;
			}
		}
		Log.Warning("File not found", "Path", text);
	}

	private void StoreDataStream(string fileName, MemoryStream stream)
	{
		string text = ToPath(fileName);
		Log.Debug("Saving file.", fileName);
		using (FileStream to = File.Create(text))
		{
			try
			{
				CopyStream(stream, to);
			}
			catch (Exception ex)
			{
				Log.Warning("Failed to save file.", "Path", text, "Exception", ex.Message, "Stack Trace", ex.StackTrace);
			}
		}
	}

	private bool FileExists(string fileName)
	{
		return File.Exists(ToPath(fileName));
	}

	private string ToPath(string fileName)
	{
		return Path.Combine(SavePath(), fileName);
	}
}
