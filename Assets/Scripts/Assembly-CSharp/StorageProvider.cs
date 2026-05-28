using System.Collections.Generic;
using System.IO;

public interface StorageProvider
{
	void Initialize();

	bool IsInitialized();

	void StoreGameData(string gameId, string gameName, string name, MemoryStream dataStream);

	string GetGameId(string name);

	void GetGameData(string name, MemoryStream dataStream);

	List<string> GetAvailableGames();

	bool HasGameData(string name);

	void DeleteGameData(string name);

	void DeleteGamesData(List<string> name);

	void Flush();

	bool HasProfile();

	void GetProfileData(MemoryStream dataStream);

	void StoreProfileData(MemoryStream dataStream);

	bool HasSettings();

	void GetSettingsData(MemoryStream dataStream);

	void StoreSettingsData(MemoryStream dataStream);
}
