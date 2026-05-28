using System;
using System.IO;
using UnityEngine;

namespace MonomiPark.SlimeRancher.Persist
{
	public class Vector3V02 : PersistedDataSet
	{
		public Vector3 value;

		public const float ROT_APPROX_TOLERANCE = 0.1f;

		public override string Identifier => "SRV3";

		public override uint Version => 2u;

		protected override void LoadData(BinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			float z = reader.ReadSingle();
			value = new Vector3(x, y, z);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(value.x);
			writer.Write(value.y);
			writer.Write(value.z);
		}

		public static Vector3V02 Load(BinaryReader reader)
		{
			Vector3V02 vector3V = new Vector3V02();
			vector3V.Load(reader.BaseStream);
			return vector3V;
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is Vector3V02)
			{
				return Equals(obj as Vector3V02);
			}
			return false;
		}

		public bool Equals(Vector3V02 v)
		{
			if (value.x == v.value.x && value.y == v.value.y)
			{
				return value.z == v.value.z;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((17 * 23 + value.x.GetHashCode()) * 23 + value.y.GetHashCode()) * 23 + value.z.GetHashCode();
		}

		public override string ToString()
		{
			return value.ToString();
		}

		public static void AssertAreEqual(Vector3V02 expected, Vector3V02 actual)
		{
		}

		public static void AssertAreApproximatelyEqual(Vector3V02 expected, Vector3V02 actual, float tolerance)
		{
		}

		internal static void AssertAreApproximatelyEqual(Vector3V02 rot1, Vector3V02 rot2, object rOT_APPROX_TOLERANCE)
		{
			throw new NotImplementedException();
		}
	}
}
