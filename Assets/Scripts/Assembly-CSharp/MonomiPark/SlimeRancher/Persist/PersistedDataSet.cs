using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MonomiPark.SlimeRancher.Persist
{
	public abstract class PersistedDataSet : Persistable
	{
		protected const short SECTION_SEPARATOR = 4096;

		protected const short ELEMENT_SEPARATOR = 8192;

		protected const short DATA_PAYLOAD_BEGIN = 12288;

		protected const short DATA_PAYLOAD_END = 16384;

		public virtual string Identifier => "";

		public virtual uint Version => 0u;

		public void Load(Stream stream)
		{
			Load(stream, skipPayloadEnd: false);
		}

		public virtual void Load(Stream stream, bool skipPayloadEnd)
		{
			BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
			if (IsValidHeader(reader))
			{
				ReadDataPayloadBegin(reader);
				LoadData(reader);
				if (!skipPayloadEnd)
				{
					ReadDataPayloadEnd(reader);
				}
				return;
			}
			throw new InvalidDataException(Log.Format("Failed to load dataset. Invalid header", "Expected Id", Identifier, "Expected Version", Version));
		}

		protected virtual bool IsValidHeader(BinaryReader reader)
		{
			string strB = reader.ReadString();
			if (Identifier.CompareTo(strB) != 0)
			{
				return false;
			}
			if (reader.ReadUInt32() != Version)
			{
				return false;
			}
			return true;
		}

		protected abstract void LoadData(BinaryReader reader);

		public long Write(Stream stream)
		{
			long position = stream.Position;
			BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8);
			WriteHeader(writer);
			WriteDataPayloadBegin(writer);
			WriteData(writer);
			WriteDataPayloadEnd(writer);
			return stream.Position - position;
		}

		protected void WriteHeader(BinaryWriter writer)
		{
			writer.Write(Identifier);
			writer.Write(Version);
		}

		protected abstract void WriteData(BinaryWriter writer);

		protected void ReadSectionSeparator(BinaryReader reader)
		{
			if (reader.ReadInt16() != 4096)
			{
				throw new Exception("Unexpected data when reading section separator.");
			}
		}

		protected void ReadElementSeparator(BinaryReader reader)
		{
			if (reader.ReadInt16() != 8192)
			{
				throw new Exception("Unexpected data when reading element separator.");
			}
		}

		protected void ReadDataPayloadBegin(BinaryReader reader)
		{
			if (reader.ReadInt16() != 12288)
			{
				throw new Exception("Unexpected data when reading beginning of data payload.");
			}
		}

		protected void ReadDataPayloadEnd(BinaryReader reader)
		{
			if (reader.ReadInt16() != 16384)
			{
				throw new Exception("Unexpected data when reading end of data payload.");
			}
		}

		protected void WriteDataPayloadBegin(BinaryWriter writer)
		{
			writer.Write((short)12288);
		}

		protected void WriteDataPayloadEnd(BinaryWriter writer)
		{
			writer.Write((short)16384);
		}

		protected void WriteElementSeparator(BinaryWriter writer)
		{
			writer.Write((short)8192);
		}

		protected void WriteSectionSeparator(BinaryWriter writer)
		{
			writer.Write((short)4096);
		}

		protected static List<T> LoadList<T>(BinaryReader reader) where T : Persistable, new()
		{
			return LoadList(reader, delegate(BinaryReader r)
			{
				T result = new T();
				result.Load(r.BaseStream);
				return result;
			});
		}

		protected static List<T> LoadList<T>(BinaryReader reader, Func<int, T> transform)
		{
			return LoadList(reader, (BinaryReader r) => transform(r.ReadInt32()));
		}

		protected static List<T> LoadList<T>(BinaryReader reader, Func<float, T> transform)
		{
			return LoadList(reader, (BinaryReader r) => transform(r.ReadSingle()));
		}

		protected static List<T> LoadList<T>(BinaryReader reader, Func<double, T> transform)
		{
			return LoadList(reader, (BinaryReader r) => transform(r.ReadDouble()));
		}

		protected static List<T> LoadList<T>(BinaryReader reader, Func<string, T> transform)
		{
			return LoadList(reader, (BinaryReader r) => transform(r.ReadString()));
		}

		protected static List<T> LoadList<T>(BinaryReader reader, Func<BinaryReader, T> readFunction)
		{
			List<T> list = new List<T>();
			for (int num = reader.ReadInt32(); num > 0; num--)
			{
				list.Add(readFunction(reader));
			}
			return list;
		}

		protected Dictionary<K, V> LoadDictionary<K, V>(BinaryReader reader, Func<BinaryReader, K> keyLoad, Func<BinaryReader, V> valueLoad) where V : new()
		{
			Dictionary<K, V> dictionary = new Dictionary<K, V>();
			for (int num = reader.ReadInt32(); num > 0; num--)
			{
				K key = keyLoad(reader);
				V value = valueLoad(reader);
				ReadElementSeparator(reader);
				dictionary.Add(key, value);
			}
			return dictionary;
		}

		protected static void WriteList<T>(BinaryWriter writer, List<T> items) where T : Persistable
		{
			WriteList(writer, items, delegate(T it)
			{
				it.Write(writer.BaseStream);
			});
		}

		protected static void WriteList<T>(BinaryWriter writer, List<T> items, Func<T, int> transform)
		{
			WriteList(writer, items, delegate(T it)
			{
				writer.Write(transform(it));
			});
		}

		protected static void WriteList<T>(BinaryWriter writer, List<T> items, Func<T, float> transform)
		{
			WriteList(writer, items, delegate(T it)
			{
				writer.Write(transform(it));
			});
		}

		protected static void WriteList<T>(BinaryWriter writer, List<T> items, Func<T, double> transform)
		{
			WriteList(writer, items, delegate(T it)
			{
				writer.Write(transform(it));
			});
		}

		protected static void WriteList<T>(BinaryWriter writer, List<T> items, Func<T, string> transform)
		{
			WriteList(writer, items, delegate(T it)
			{
				writer.Write(transform(it));
			});
		}

		protected static void WriteList<T>(BinaryWriter writer, List<T> items, Action<T> writeFunction)
		{
			writer.Write(items.Count);
			foreach (T item in items)
			{
				writeFunction(item);
			}
		}

		protected void WriteDictionary<K, V>(BinaryWriter writer, Dictionary<K, V> dict, Action<BinaryWriter, K> keyAction, Action<BinaryWriter, V> valueAction)
		{
			writer.Write(dict.Count);
			foreach (KeyValuePair<K, V> item in dict)
			{
				keyAction(writer, item.Key);
				valueAction(writer, item.Value);
				WriteElementSeparator(writer);
			}
		}

		protected void WriteNullable(BinaryWriter writer, double? nullable)
		{
			writer.Write(nullable.HasValue);
			if (nullable.HasValue)
			{
				writer.Write(nullable.Value);
			}
		}

		protected void WriteNullable(BinaryWriter writer, float? nullable)
		{
			writer.Write(nullable.HasValue);
			if (nullable.HasValue)
			{
				writer.Write(nullable.Value);
			}
		}

		protected void LoadNullable(BinaryReader reader, out double? nullable)
		{
			if (reader.ReadBoolean())
			{
				nullable = reader.ReadDouble();
			}
			else
			{
				nullable = null;
			}
		}

		protected void LoadNullable(BinaryReader reader, out float? nullable)
		{
			if (reader.ReadBoolean())
			{
				nullable = reader.ReadSingle();
			}
			else
			{
				nullable = null;
			}
		}

		protected static T LoadPersistable<T>(BinaryReader reader) where T : class, Persistable, new()
		{
			if (reader.ReadBoolean())
			{
				T val = new T();
				val.Load(reader.BaseStream);
				return val;
			}
			return null;
		}

		protected static void WritePersistable<T>(BinaryWriter writer, T instance) where T : class, Persistable
		{
			writer.Write(instance != null);
			instance?.Write(writer.BaseStream);
		}

		protected static Dictionary<string, T> UpgradeFrom<T>(Dictionary<Vector3V02, T> legacy, Dictionary<Vector3, string> mapping)
		{
			if (legacy == null)
			{
				return null;
			}
			Dictionary<string, T> dictionary = new Dictionary<string, T>();
			foreach (KeyValuePair<Vector3V02, T> pair in legacy)
			{
				try
				{
					dictionary[mapping.First((KeyValuePair<Vector3, string> mp) => (pair.Key.value - mp.Key).sqrMagnitude < 100f).Value] = pair.Value;
				}
				catch (InvalidOperationException)
				{
					Log.Warning("Failed to get id from position.", "position", pair.Key.value, "type", typeof(T));
				}
			}
			return dictionary;
		}
	}
}
