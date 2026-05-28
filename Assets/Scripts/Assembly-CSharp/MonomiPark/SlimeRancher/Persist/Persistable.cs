using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public interface Persistable
	{
		void Load(Stream stream);

		long Write(Stream stream);
	}
}
