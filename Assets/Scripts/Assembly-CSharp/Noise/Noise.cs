using System;
using UnityEngine;

namespace Noise
{
	public static class Noise
	{
		private struct Grad
		{
			public double x;

			public double y;

			public double z;

			public double w;

			public Grad(double x, double y, double z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
				w = 0.0;
			}
		}

		private static Grad[] grad3;

		private static short[] p;

		private static short[] perm;

		private static short[] permMod12;

		private static double F3;

		private static double G3;

		static Noise()
		{
			grad3 = new Grad[12]
			{
				new Grad(1.0, 1.0, 0.0),
				new Grad(-1.0, 1.0, 0.0),
				new Grad(1.0, -1.0, 0.0),
				new Grad(-1.0, -1.0, 0.0),
				new Grad(1.0, 0.0, 1.0),
				new Grad(-1.0, 0.0, 1.0),
				new Grad(1.0, 0.0, -1.0),
				new Grad(-1.0, 0.0, -1.0),
				new Grad(0.0, 1.0, 1.0),
				new Grad(0.0, -1.0, 1.0),
				new Grad(0.0, 1.0, -1.0),
				new Grad(0.0, -1.0, -1.0)
			};
			p = new short[256]
			{
				151, 160, 137, 91, 90, 15, 131, 13, 201, 95,
				96, 53, 194, 233, 7, 225, 140, 36, 103, 30,
				69, 142, 8, 99, 37, 240, 21, 10, 23, 190,
				6, 148, 247, 120, 234, 75, 0, 26, 197, 62,
				94, 252, 219, 203, 117, 35, 11, 32, 57, 177,
				33, 88, 237, 149, 56, 87, 174, 20, 125, 136,
				171, 168, 68, 175, 74, 165, 71, 134, 139, 48,
				27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
				60, 211, 133, 230, 220, 105, 92, 41, 55, 46,
				245, 40, 244, 102, 143, 54, 65, 25, 63, 161,
				1, 216, 80, 73, 209, 76, 132, 187, 208, 89,
				18, 169, 200, 196, 135, 130, 116, 188, 159, 86,
				164, 100, 109, 198, 173, 186, 3, 64, 52, 217,
				226, 250, 124, 123, 5, 202, 38, 147, 118, 126,
				255, 82, 85, 212, 207, 206, 59, 227, 47, 16,
				58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
				119, 248, 152, 2, 44, 154, 163, 70, 221, 153,
				101, 155, 167, 43, 172, 9, 129, 22, 39, 253,
				19, 98, 108, 110, 79, 113, 224, 232, 178, 185,
				112, 104, 218, 246, 97, 228, 251, 34, 242, 193,
				238, 210, 144, 12, 191, 179, 162, 241, 81, 51,
				145, 235, 249, 14, 239, 107, 49, 192, 214, 31,
				181, 199, 106, 157, 184, 84, 204, 176, 115, 121,
				50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
				222, 114, 67, 29, 24, 72, 243, 141, 128, 195,
				78, 66, 215, 61, 156, 180
			};
			perm = new short[512];
			permMod12 = new short[512];
			F3 = 1.0 / 3.0;
			G3 = 1.0 / 6.0;
			for (int i = 0; i < 512; i++)
			{
				perm[i] = p[i & 0xFF];
				permMod12[i] = (short)(perm[i] % 12);
			}
		}

		private static int fastfloor(double x)
		{
			int num = (int)x;
			if (!(x < (double)num))
			{
				return num;
			}
			return num - 1;
		}

		private static double dot(Grad g, double x, double y, double z)
		{
			return g.x * x + g.y * y + g.z * z;
		}

		public static float PerlinNoise(double x, float y, float z, float scale, float height, float power)
		{
			float noise = GetNoise(x / (double)scale, (double)y / (double)scale, (double)z / (double)scale);
			noise *= height;
			if (power != 0f)
			{
				noise = Mathf.Pow(noise, power);
			}
			return noise;
		}

		public static float GetNoise(double xin, double yin, double zin)
		{
			double num = (xin + yin + zin) * F3;
			int num2 = fastfloor(xin + num);
			int num3 = fastfloor(yin + num);
			int num4 = fastfloor(zin + num);
			double num5 = (double)(num2 + num3 + num4) * G3;
			double num6 = (double)num2 - num5;
			double num7 = (double)num3 - num5;
			double num8 = (double)num4 - num5;
			double num9 = xin - num6;
			double num10 = yin - num7;
			double num11 = zin - num8;
			int num12;
			int num13;
			int num14;
			int num15;
			int num16;
			int num17;
			if (num9 >= num10)
			{
				if (num10 >= num11)
				{
					num12 = 1;
					num13 = 0;
					num14 = 0;
					num15 = 1;
					num16 = 1;
					num17 = 0;
				}
				else if (num9 >= num11)
				{
					num12 = 1;
					num13 = 0;
					num14 = 0;
					num15 = 1;
					num16 = 0;
					num17 = 1;
				}
				else
				{
					num12 = 0;
					num13 = 0;
					num14 = 1;
					num15 = 1;
					num16 = 0;
					num17 = 1;
				}
			}
			else if (num10 < num11)
			{
				num12 = 0;
				num13 = 0;
				num14 = 1;
				num15 = 0;
				num16 = 1;
				num17 = 1;
			}
			else if (num9 < num11)
			{
				num12 = 0;
				num13 = 1;
				num14 = 0;
				num15 = 0;
				num16 = 1;
				num17 = 1;
			}
			else
			{
				num12 = 0;
				num13 = 1;
				num14 = 0;
				num15 = 1;
				num16 = 1;
				num17 = 0;
			}
			double num18 = num9 - (double)num12 + G3;
			double num19 = num10 - (double)num13 + G3;
			double num20 = num11 - (double)num14 + G3;
			double num21 = num9 - (double)num15 + 2.0 * G3;
			double num22 = num10 - (double)num16 + 2.0 * G3;
			double num23 = num11 - (double)num17 + 2.0 * G3;
			double num24 = num9 - 1.0 + 3.0 * G3;
			double num25 = num10 - 1.0 + 3.0 * G3;
			double num26 = num11 - 1.0 + 3.0 * G3;
			int num27 = num2 & 0xFF;
			int num28 = num3 & 0xFF;
			int num29 = num4 & 0xFF;
			int num30 = permMod12[num27 + perm[num28 + perm[num29]]];
			int num31 = permMod12[num27 + num12 + perm[num28 + num13 + perm[num29 + num14]]];
			int num32 = permMod12[num27 + num15 + perm[num28 + num16 + perm[num29 + num17]]];
			int num33 = permMod12[num27 + 1 + perm[num28 + 1 + perm[num29 + 1]]];
			double num34 = 0.6 - num9 * num9 - num10 * num10 - num11 * num11;
			double num35;
			if (num34 < 0.0)
			{
				num35 = 0.0;
			}
			else
			{
				num34 *= num34;
				num35 = num34 * num34 * dot(grad3[num30], num9, num10, num11);
			}
			double num36 = 0.6 - num18 * num18 - num19 * num19 - num20 * num20;
			double num37;
			if (num36 < 0.0)
			{
				num37 = 0.0;
			}
			else
			{
				num36 *= num36;
				num37 = num36 * num36 * dot(grad3[num31], num18, num19, num20);
			}
			double num38 = 0.6 - num21 * num21 - num22 * num22 - num23 * num23;
			double num39;
			if (num38 < 0.0)
			{
				num39 = 0.0;
			}
			else
			{
				num38 *= num38;
				num39 = num38 * num38 * dot(grad3[num32], num21, num22, num23);
			}
			double num40 = 0.6 - num24 * num24 - num25 * num25 - num26 * num26;
			double num41;
			if (num40 < 0.0)
			{
				num41 = 0.0;
			}
			else
			{
				num40 *= num40;
				num41 = num40 * num40 * dot(grad3[num33], num24, num25, num26);
			}
			return (float)(32.0 * (num35 + num37 + num39 + num41) + 1.0) * 0.5f;
		}

		public static float GetOctaveNoise(double pX, double pY, double pZ, int pOctaves)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			for (int i = 0; i < pOctaves; i++)
			{
				num3 = (float)Math.Pow(0.5, i);
				num4 = (float)Math.Pow(2.0, i);
				num += GetNoise(pX * (double)num4, pY * (double)num4, pZ) * num3;
				num2 += num3;
			}
			return num / num2;
		}
	}
}
