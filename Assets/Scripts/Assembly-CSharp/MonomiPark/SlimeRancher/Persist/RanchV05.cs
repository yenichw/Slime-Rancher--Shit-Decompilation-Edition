using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonomiPark.SlimeRancher.Persist
{
	public class RanchV05 : VersionedPersistedDataSet<RanchV04>
	{
		public List<LandPlotV04> plots;

		public Dictionary<string, AccessDoor.State> accessDoorStates;

		public override string Identifier => "SRRANCH";

		public override uint Version => 5u;

		public RanchV05()
		{
		}

		public RanchV05(RanchV04 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			plots = PersistedDataSet.LoadList<LandPlotV04>(reader);
			accessDoorStates = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => (AccessDoor.State)r.ReadInt32());
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WriteList(writer, plots);
			WriteDictionary(writer, accessDoorStates, delegate(BinaryWriter w, string k)
			{
				w.Write(k);
			}, delegate(BinaryWriter w, AccessDoor.State v)
			{
				w.Write((int)v);
			});
		}

		public static RanchV05 Load(BinaryReader reader)
		{
			RanchV05 ranchV = new RanchV05();
			ranchV.Load(reader.BaseStream);
			return ranchV;
		}

		protected override void UpgradeFrom(RanchV04 legacyData)
		{
			plots = LandPlotV04.UpgradeList(legacyData.plots);
			accessDoorStates = UpgradeDoorsFrom(legacyData.accessDoorStates);
		}

		public static Dictionary<string, AccessDoor.State> UpgradeDoorsFrom(Dictionary<Vector3V02, AccessDoor.State> legacyData)
		{
			Dictionary<Vector3, string> dictionary = new Dictionary<Vector3, string>();
			dictionary[new Vector3(46.9f, 12.3f, -102.2f)] = "door0308232953";
			dictionary[new Vector3(-251.6f, 10.4f, 14.6f)] = "door1089650386";
			dictionary[new Vector3(162.3f, 12.4f, -133.9f)] = "door0231652753";
			dictionary[new Vector3(25.3f, 13.6f, 181.6f)] = "door0646655257";
			dictionary[new Vector3(-173.5f, 2.3f, 331.7f)] = "door1946130400";
			Dictionary<string, AccessDoor.State> dictionary2 = new Dictionary<string, AccessDoor.State>();
			foreach (KeyValuePair<Vector3V02, AccessDoor.State> legacyDatum in legacyData)
			{
				Vector3 value = legacyDatum.Key.value;
				bool flag = false;
				foreach (KeyValuePair<Vector3, string> item in dictionary)
				{
					if ((value - item.Key).sqrMagnitude < 100f)
					{
						dictionary2[item.Value] = legacyDatum.Value;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Log.Warning("Failed to find gordo match during upgrade: " + value);
				}
			}
			return dictionary2;
		}

		public static void AssertAreEqual(RanchV05 expected, RanchV05 actual)
		{
			for (int i = 0; i < expected.plots.Count; i++)
			{
				LandPlotV04.AssertAreEqual(expected.plots[i], actual.plots[i]);
			}
			TestUtil.AssertAreEqual(expected.accessDoorStates, actual.accessDoorStates, delegate
			{
			}, "accessDoorStates");
		}

		public static void AssertAreEqual(RanchV04 expected, RanchV05 actual)
		{
			for (int i = 0; i < expected.plots.Count; i++)
			{
				LandPlotV04.AssertAreEqual(expected.plots[i], actual.plots[i]);
			}
			TestUtil.AssertAreEqual(UpgradeDoorsFrom(expected.accessDoorStates), actual.accessDoorStates, delegate
			{
			}, "accessDoorStates");
		}
	}
}
