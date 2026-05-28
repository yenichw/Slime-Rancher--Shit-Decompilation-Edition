using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AssetStorageProvider : StorageProvider
{
	private string directory;

	public AssetStorageProvider(string directory)
	{
		this.directory = directory;
	}

	public void Initialize()
	{
	}

	public bool IsInitialized()
	{
		return true;
	}

	public List<string> GetAvailableGames()
	{
		return (from r in Resources.LoadAll<TextAsset>(directory)
			select r.name).ToList();
	}

	public bool HasGameData(string name)
	{
		return LoadAsset(name) != null;
	}

	public void GetGameData(string name, MemoryStream stream)
	{
		TextAsset textAsset = LoadAsset(name);
		stream.Write(textAsset.bytes, 0, textAsset.bytes.Length);
	}

	public string GetGameId(string name)
	{
		return string.Empty;
	}

	public void StoreGameData(string gameId, string gameName, string name, MemoryStream stream)
	{
	}

	public void DeleteGameData(string name)
	{
	}

	public void DeleteGamesData(List<string> name)
	{
	}

	public bool HasProfile()
	{
		return false;
	}

	public void GetProfileData(MemoryStream stream)
	{
	}

	public void StoreProfileData(MemoryStream stream)
	{
	}

	public bool HasSettings()
	{
		return false;
	}

	public void GetSettingsData(MemoryStream stream)
	{
	}

	public void StoreSettingsData(MemoryStream stream)
	{
	}

	public void Flush()
	{
	}

	private TextAsset LoadAsset(string name)
	{
		return Resources.Load<TextAsset>($"{directory}/{name}");
	}
}
