namespace Noise
{
	public class NoiseGen
	{
		public double XScale = 0.02;

		public double YScale = 0.02;

		public double ZScale = 1.0;

		public byte Octaves = 1;

		public double Scale
		{
			set
			{
				XScale = value;
				YScale = value;
			}
		}

		public NoiseGen()
		{
		}

		public NoiseGen(double pScale, byte pOctaves)
		{
			XScale = pScale;
			YScale = pScale;
			Octaves = pOctaves;
		}

		public NoiseGen(double pXScale, double pYScale, byte pOctaves)
		{
			XScale = pXScale;
			YScale = pYScale;
			Octaves = pOctaves;
		}

		public float GetNoise(double x, double y, double z)
		{
			if (Octaves > 1)
			{
				return Noise.GetOctaveNoise(x * XScale, y * YScale, z * ZScale, Octaves);
			}
			return Noise.GetNoise(x * XScale, y * YScale, z * ZScale);
		}
	}
}
