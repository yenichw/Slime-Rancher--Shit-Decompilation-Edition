using System;
using System.Runtime.Serialization;
using UnityEngine;

public class Vector3Surrogate : ISerializationSurrogate
{
	public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
	{
		Vector3 vector = (Vector3)obj;
		info.AddValue("x", vector.x);
		info.AddValue("y", vector.y);
		info.AddValue("z", vector.z);
	}

	public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
	{
		Vector3 vector = (Vector3)obj;
		try
		{
			vector.x = (float)info.GetDecimal("x");
			vector.y = (float)info.GetDecimal("y");
			vector.z = (float)info.GetDecimal("z");
		}
		catch (Exception)
		{
			Debug.Log("Failed to load vector data, setting to starting pos");
			vector.x = 88.21f;
			vector.y = 16.41f;
			vector.z = -139.86f;
		}
		return vector;
	}
}
