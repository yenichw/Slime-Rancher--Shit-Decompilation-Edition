using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MonomiPark.SlimeRancher.Persist;
using UnityEngine;

public class ProfileData : Persistable
{
	public const int CURR_FORMAT_ID = 3;

	public const int MIN_HANDLED_FORMAT_ID = 1;

	public int optionsFormatID = 2;

	public OptionsData options = new OptionsData();

	public AchieveData achieve = new AchieveData();

	public string continueGameName;

	private const string NAME = "slimerancher.prf";

	private static BinaryFormatter CreateFormatter()
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		SurrogateSelector surrogateSelector = new SurrogateSelector();
		surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), new Vector3Surrogate());
		binaryFormatter.SurrogateSelector = surrogateSelector;
		return binaryFormatter;
	}

	public void Load(Stream stream)
	{
		BinaryFormatter binaryFormatter = CreateFormatter();
		int num = (int)binaryFormatter.Deserialize(stream);
		if (num > 3)
		{
			Debug.Log("File format newer than current version type=ProfileData fileVer=" + num + " currVer=" + 3);
			throw new VersionMismatchException("File format newer than current version.");
		}
		if (num < 1)
		{
			Debug.Log("Unhandled version type=ProfileData fileVer=" + num + " currVer=" + 3);
			throw new VersionMismatchException("File format unhandled.");
		}
		options = DataModule<OptionsData>.Deserialize(binaryFormatter, stream, 2);
		achieve = DataModule<AchieveData>.Deserialize(binaryFormatter, stream, 2);
		try
		{
			continueGameName = (string)binaryFormatter.Deserialize(stream);
		}
		catch (EndOfStreamException)
		{
			continueGameName = null;
		}
	}

	public long Write(Stream stream)
	{
		throw new NotImplementedException("Write is not supported for legacy data.");
	}
}
