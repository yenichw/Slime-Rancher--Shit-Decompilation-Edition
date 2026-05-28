using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class ObjectCopier
{
	public static T Clone<T>(T source)
	{
		if (!typeof(T).IsSerializable)
		{
			Debug.Log("The type must be serializable.");
		}
		if (source == null)
		{
			return default(T);
		}
		IFormatter formatter = new BinaryFormatter();
		SurrogateSelector surrogateSelector = new SurrogateSelector();
		surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), new Vector3Surrogate());
		formatter.SurrogateSelector = surrogateSelector;
		Stream stream = new MemoryStream();
		using (stream)
		{
			formatter.Serialize(stream, source);
			stream.Seek(0L, SeekOrigin.Begin);
			return (T)formatter.Deserialize(stream);
		}
	}
}
