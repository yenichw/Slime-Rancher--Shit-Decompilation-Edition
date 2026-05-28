using System;
using System.Collections.Generic;
using System.IO;

public class EmptyStorageProvider : StorageProvider, IDisposable
{
	public void DeleteGameData(string name)
	{
	}

	public void DeleteGamesData(List<string> names)
	{
	}

	public void Dispose()
	{
	}

	public List<string> GetAvailableGames()
	{
		return new List<string>();
	}

	public string GetGameId(string name)
	{
		return string.Empty;
	}

	public void GetGameData(string name, MemoryStream dataStream)
	{
	}

	public void GetProfileData(MemoryStream dataStream)
	{
	}

	public void GetSettingsData(MemoryStream dataStream)
	{
	}

	public bool HasGameData(string name)
	{
		return false;
	}

	public bool HasProfile()
	{
		return false;
	}

	public bool HasSettings()
	{
		return false;
	}

	public void Initialize()
	{
	}

	public bool IsInitialized()
	{
		return true;
	}

	public void StoreGameData(string gameId, string gameName, string name, MemoryStream dataStream)
	{
	}

	public void StoreProfileData(MemoryStream dataStream)
	{
	}

	public void StoreSettingsData(MemoryStream dataStream)
	{
	}

	public void Flush()
	{
	}
}
