using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class ActorDataV05 : VersionedPersistedDataSet<ActorDataV04>
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

		public List<Identifiable.Id> fashions = new List<Identifiable.Id>();

		public override string Identifier => "SRAD";

		public override uint Version => 5u;

		public ActorDataV05()
		{
		}

		public ActorDataV05(ActorDataV04 legacyData)
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
			fashions = PersistedDataSet.LoadList(reader, (int v) => (Identifiable.Id)v);
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
			PersistedDataSet.WriteList(writer, fashions, (Identifiable.Id v) => (int)v);
		}

		public static void AssertAreEqual(ActorDataV05 expected, ActorDataV05 actual)
		{
			Vector3V02.AssertAreEqual(expected.pos, actual.pos);
			Vector3V02.AssertAreEqual(expected.rot, actual.rot);
			ResourceCycleDataV03.AssertAreEqual(expected.cycleData, actual.cycleData);
			SlimeEmotionDataV02.AssertAreEqual(expected.emotions, actual.emotions);
			TestUtil.AssertAreEqual(expected.fashions, actual.fashions, "fashions");
		}

		public static void AssertAreEqual(ActorDataV04 expected, ActorDataV05 actual)
		{
			Vector3V02.AssertAreEqual(expected.pos, actual.pos);
			Vector3V02.AssertAreEqual(expected.rot, actual.rot);
			ResourceCycleDataV03.AssertAreEqual(expected.cycleData, actual.cycleData);
			SlimeEmotionDataV02.AssertAreEqual(expected.emotions, actual.emotions);
			TestUtil.AssertAreEqual(new List<Identifiable.Id>(), actual.fashions, "fashions");
		}

		protected override void UpgradeFrom(ActorDataV04 legacyData)
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
			fashions = new List<Identifiable.Id>();
		}
	}
}
