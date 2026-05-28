using System.IO;
using System.Text;

namespace MonomiPark.SlimeRancher.Persist
{
	public abstract class VersionedPersistedDataSet<T> : PersistedDataSet where T : Persistable, new()
	{
		public override void Load(Stream stream, bool skipPayloadEnd)
		{
			BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
			long position = stream.Position;
			bool num = IsValidHeader(reader);
			stream.Seek(position, SeekOrigin.Begin);
			if (num)
			{
				base.Load(stream, skipPayloadEnd);
			}
			else
			{
				UpgradeFrom(LoadLegacy(new T(), stream, skipPayloadEnd));
			}
		}

		protected virtual T LoadLegacy(T instance, Stream stream, bool skipPayloadEnd)
		{
			if (instance is PersistedDataSet persistedDataSet)
			{
				persistedDataSet.Load(stream, skipPayloadEnd);
			}
			else
			{
				instance.Load(stream);
			}
			return instance;
		}

		protected abstract void UpgradeFrom(T legacyData);
	}
}
