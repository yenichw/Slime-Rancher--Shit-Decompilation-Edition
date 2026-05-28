using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class ActorDataV04 : VersionedPersistedDataSet<ActorDataV03>
	{
		public Vector3V02 pos;

		public Vector3V02 rot;

		public int id;

		public SlimeEmotionDataV02 emotions;

		public double transformTime;

		public double reproduceTime;

		public ResourceCycleDataV03 cycleData;

		public double? disabledAtTime;

		public bool isFeral;

		public override string Identifier => "SRAD";

		public override uint Version => 4u;

		public ActorDataV04()
		{
		}

		public ActorDataV04(ActorDataV03 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			pos = new Vector3V02();
			pos.Load(reader.BaseStream);
			rot = new Vector3V02();
			rot.Load(reader.BaseStream);
			id = reader.ReadInt32();
			emotions = new SlimeEmotionDataV02();
			emotions.Load(reader.BaseStream);
			transformTime = reader.ReadDouble();
			reproduceTime = reader.ReadDouble();
			cycleData = new ResourceCycleDataV03();
			cycleData.Load(reader.BaseStream);
			if (reader.ReadBoolean())
			{
				disabledAtTime = reader.ReadDouble();
			}
			isFeral = reader.ReadBoolean();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			pos.Write(writer.BaseStream);
			rot.Write(writer.BaseStream);
			writer.Write(id);
			emotions.Write(writer.BaseStream);
			writer.Write(transformTime);
			writer.Write(reproduceTime);
			cycleData.Write(writer.BaseStream);
			if (disabledAtTime.HasValue)
			{
				writer.Write(value: true);
				writer.Write(disabledAtTime.Value);
			}
			else
			{
				writer.Write(value: false);
			}
			writer.Write(isFeral);
		}

		public static void AssertAreEqual(ActorDataV04 expected, ActorDataV04 actual)
		{
			Vector3V02.AssertAreEqual(expected.pos, actual.pos);
			Vector3V02.AssertAreEqual(expected.rot, actual.rot);
			ResourceCycleDataV03.AssertAreEqual(expected.cycleData, actual.cycleData);
			SlimeEmotionDataV02.AssertAreEqual(expected.emotions, actual.emotions);
		}

		public static void AssertAreEqual(ActorDataV03 expected, ActorDataV04 actual)
		{
			Vector3V02.AssertAreEqual(expected.pos, actual.pos);
			Vector3V02.AssertAreEqual(expected.rot, actual.rot);
			ResourceCycleDataV03.AssertAreEqual(expected.cycleData, actual.cycleData);
			SlimeEmotionDataV02.AssertAreEqual(expected.emotions, actual.emotions);
		}

		protected override void UpgradeFrom(ActorDataV03 legacyData)
		{
			pos = legacyData.pos;
			rot = legacyData.rot;
			id = legacyData.id;
			emotions = legacyData.emotions;
			transformTime = legacyData.transformTime;
			reproduceTime = legacyData.reproduceTime;
			cycleData = legacyData.cycleData;
			disabledAtTime = legacyData.disabledAtTime;
			isFeral = legacyData.isFeral;
		}
	}
}
