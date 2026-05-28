using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

[Serializable]
public class SlimeEmotionData : Dictionary<SlimeEmotions.Emotion, float>
{
	private class EmotionComparer : IEqualityComparer<KeyValuePair<SlimeEmotions.Emotion, float>>
	{
		public bool Equals(KeyValuePair<SlimeEmotions.Emotion, float> x, KeyValuePair<SlimeEmotions.Emotion, float> y)
		{
			if (x.Key == y.Key)
			{
				return Math.Abs(x.Value - y.Value) < 0.001f;
			}
			return false;
		}

		public int GetHashCode(KeyValuePair<SlimeEmotions.Emotion, float> obj)
		{
			throw new NotImplementedException();
		}
	}

	public SlimeEmotionData()
	{
	}

	public SlimeEmotionData(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public SlimeEmotionData(SlimeEmotions emotions)
	{
		foreach (SlimeEmotions.EmotionState item in emotions.GetAll())
		{
			base[item.emotion] = item.currVal;
		}
	}

	public void AverageIn(SlimeEmotions emotions, float weight)
	{
		float num = 1f - weight;
		foreach (SlimeEmotions.EmotionState item in emotions.GetAll())
		{
			base[item.emotion] = base[item.emotion] * num + item.currVal * weight;
		}
	}

	public override bool Equals(object o)
	{
		if (!(o is SlimeEmotionData))
		{
			return false;
		}
		SlimeEmotionData second = (SlimeEmotionData)o;
		return this.SequenceEqual(second, new EmotionComparer());
	}

	public override int GetHashCode()
	{
		int num = 0;
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<SlimeEmotions.Emotion, float> current = enumerator.Current;
				num ^= current.Key.GetHashCode() ^ current.Value.GetHashCode();
			}
			return num;
		}
	}

	public override string ToString()
	{
		string text = "";
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<SlimeEmotions.Emotion, float> current = enumerator.Current;
				text = string.Concat(text, current.Key, ":", current.Value, ",");
			}
			return text;
		}
	}
}
