using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class SlimeEmotionDataV02 : PersistedDataSet
	{
		public Dictionary<SlimeEmotions.Emotion, float> emotionData = new Dictionary<SlimeEmotions.Emotion, float>();

		public override string Identifier => "SRSED";

		public override uint Version => 2u;

		protected override void LoadData(BinaryReader reader)
		{
			emotionData = new Dictionary<SlimeEmotions.Emotion, float>();
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				SlimeEmotions.Emotion key = (SlimeEmotions.Emotion)reader.ReadInt32();
				float value = reader.ReadSingle();
				emotionData.Add(key, value);
			}
		}

		protected override void WriteData(BinaryWriter writer)
		{
			if (emotionData == null)
			{
				writer.Write(0);
				return;
			}
			writer.Write(emotionData.Count);
			foreach (KeyValuePair<SlimeEmotions.Emotion, float> emotionDatum in emotionData)
			{
				writer.Write((int)emotionDatum.Key);
				writer.Write(emotionDatum.Value);
			}
		}

		public static void AssertAreEqual(SlimeEmotionDataV02 expected, SlimeEmotionDataV02 actual)
		{
			foreach (KeyValuePair<SlimeEmotions.Emotion, float> emotionDatum in expected.emotionData)
			{
				_ = emotionDatum;
			}
		}
	}
}
