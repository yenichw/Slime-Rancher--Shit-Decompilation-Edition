using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public abstract class DataModule<T> where T : DataModule<T>
{
	public void Serialize(BinaryFormatter formatter, FileStream file, int currFormatID)
	{
		formatter.Serialize(file, currFormatID);
		formatter.Serialize(file, this);
	}

	public static T Deserialize(BinaryFormatter formatter, Stream file, int currFormatID)
	{
		int num = (int)formatter.Deserialize(file);
		if (num > currFormatID)
		{
			Debug.Log(string.Concat("File format newer than current version type=", typeof(T), " fileVer=", num, " currVer=", currFormatID));
			throw new VersionMismatchException("File format newer than current version.");
		}
		if (num < currFormatID)
		{
			Debug.Log(string.Concat("Unhandled version type=", typeof(T), " fileVer=", num, " currVer=", currFormatID));
			throw new VersionMismatchException("File format unhandled.");
		}
		return formatter.Deserialize(file) as T;
	}
}
