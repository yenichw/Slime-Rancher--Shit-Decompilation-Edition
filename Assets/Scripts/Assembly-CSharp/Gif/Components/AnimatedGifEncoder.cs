using System;
using System.IO;
using UnityEngine;

namespace Gif.Components
{
	public class AnimatedGifEncoder
	{
		protected int width;

		protected int height;

		protected Color32 transparent = new Color32(0, 0, 0, 0);

		protected int transIndex;

		protected int repeat = -1;

		protected int delay;

		protected bool started;

		protected FileStream fs;

		protected byte[] pixels;

		protected byte[] indexedPixels;

		protected int colorDepth;

		protected byte[] colorTab;

		protected bool[] usedEntry = new bool[256];

		protected int palSize = 7;

		protected int dispose = -1;

		protected bool closeStream;

		protected bool firstFrame = true;

		protected bool sizeSet;

		protected int sample = 10;

		public void SetDelay(int ms)
		{
			delay = (int)Math.Round((float)ms / 10f);
		}

		public void SetDispose(int code)
		{
			if (code >= 0)
			{
				dispose = code;
			}
		}

		public void SetRepeat(int iter)
		{
			if (iter >= 0)
			{
				repeat = iter;
			}
		}

		public void SetTransparent(Color32 c)
		{
			transparent = c;
		}

		public bool AddFrame(Color32[] pixels, int imgWidth, int imgHeight)
		{
			if (pixels == null || !started)
			{
				return false;
			}
			bool result = true;
			try
			{
				if (!sizeSet)
				{
					SetSize(imgWidth, imgHeight);
				}
				this.pixels = ConvertToBytePixels(pixels);
				AnalyzePixels();
				if (firstFrame)
				{
					WriteLSD();
					WritePalette();
					if (repeat >= 0)
					{
						WriteNetscapeExt();
					}
				}
				WriteGraphicCtrlExt();
				WriteImageDesc();
				if (!firstFrame)
				{
					WritePalette();
				}
				WritePixels();
				firstFrame = false;
			}
			catch (IOException)
			{
				result = false;
			}
			return result;
		}

		private byte[] ConvertToBytePixels(Color32[] intPixels)
		{
			byte[] array = new byte[intPixels.Length * 3];
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					int num = (j + i * width) * 3;
					int num2 = j + (height - i - 1) * width;
					array[num] = intPixels[num2].r;
					array[num + 1] = intPixels[num2].g;
					array[num + 2] = intPixels[num2].b;
				}
			}
			return array;
		}

		public bool Finish()
		{
			if (!started)
			{
				return false;
			}
			bool result = true;
			started = false;
			try
			{
				fs.WriteByte(59);
				fs.Flush();
				if (closeStream)
				{
					fs.Close();
				}
			}
			catch (IOException)
			{
				result = false;
			}
			transIndex = 0;
			fs = null;
			pixels = null;
			indexedPixels = null;
			colorTab = null;
			closeStream = false;
			firstFrame = true;
			return result;
		}

		public void SetFrameRate(float fps)
		{
			if (fps != 0f)
			{
				delay = (int)Math.Round(100f / fps);
			}
		}

		public void SetQuality(int quality)
		{
			if (quality < 1)
			{
				quality = 1;
			}
			sample = quality;
		}

		public void SetSize(int w, int h)
		{
			if (!started || firstFrame)
			{
				width = w;
				height = h;
				if (width < 1)
				{
					width = 320;
				}
				if (height < 1)
				{
					height = 240;
				}
				sizeSet = true;
			}
		}

		public bool Start(FileStream os)
		{
			if (os == null)
			{
				return false;
			}
			bool flag = true;
			closeStream = false;
			fs = os;
			try
			{
				WriteString("GIF89a");
			}
			catch (IOException)
			{
				flag = false;
			}
			return started = flag;
		}

		public bool Start(string file)
		{
			bool flag = true;
			try
			{
				fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
				flag = Start(fs);
				closeStream = true;
			}
			catch (IOException)
			{
				flag = false;
			}
			return started = flag;
		}

		protected void AnalyzePixels()
		{
			int num = pixels.Length;
			int num2 = num / 3;
			indexedPixels = new byte[num2];
			NeuQuant neuQuant = new NeuQuant(pixels, num, sample);
			colorTab = neuQuant.Process();
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				int num4 = neuQuant.Map(pixels[num3++] & 0xFF, pixels[num3++] & 0xFF, pixels[num3++] & 0xFF);
				usedEntry[num4] = true;
				indexedPixels[i] = (byte)num4;
			}
			pixels = null;
			colorDepth = 8;
			palSize = 7;
			if (transparent != Color.clear)
			{
				transIndex = FindClosest(transparent);
			}
		}

		protected int FindClosest(Color32 c)
		{
			if (colorTab == null)
			{
				return -1;
			}
			int r = c.r;
			int g = c.g;
			int b = c.b;
			int result = 0;
			int num = 16777216;
			int num2 = colorTab.Length;
			for (int i = 0; i < num2; i++)
			{
				int num3 = r - (colorTab[i++] & 0xFF);
				int num4 = g - (colorTab[i++] & 0xFF);
				int num5 = b - (colorTab[i] & 0xFF);
				int num6 = num3 * num3 + num4 * num4 + num5 * num5;
				int num7 = i / 3;
				if (usedEntry[num7] && num6 < num)
				{
					num = num6;
					result = num7;
				}
			}
			return result;
		}

		protected void WriteGraphicCtrlExt()
		{
			fs.WriteByte(33);
			fs.WriteByte(249);
			fs.WriteByte(4);
			int num;
			int num2;
			if (transparent == Color.clear)
			{
				num = 0;
				num2 = 0;
			}
			else
			{
				num = 1;
				num2 = 2;
			}
			if (dispose >= 0)
			{
				num2 = dispose & 7;
			}
			num2 <<= 2;
			fs.WriteByte(Convert.ToByte(0 | num2 | 0 | num));
			WriteShort(delay);
			fs.WriteByte(Convert.ToByte(transIndex));
			fs.WriteByte(0);
		}

		protected void WriteImageDesc()
		{
			fs.WriteByte(44);
			WriteShort(0);
			WriteShort(0);
			WriteShort(width);
			WriteShort(height);
			if (firstFrame)
			{
				fs.WriteByte(0);
			}
			else
			{
				fs.WriteByte(Convert.ToByte(0x80 | palSize));
			}
		}

		protected void WriteLSD()
		{
			WriteShort(width);
			WriteShort(height);
			fs.WriteByte(Convert.ToByte(0xF0 | palSize));
			fs.WriteByte(0);
			fs.WriteByte(0);
		}

		protected void WriteNetscapeExt()
		{
			fs.WriteByte(33);
			fs.WriteByte(byte.MaxValue);
			fs.WriteByte(11);
			WriteString("NETSCAPE2.0");
			fs.WriteByte(3);
			fs.WriteByte(1);
			WriteShort(repeat);
			fs.WriteByte(0);
		}

		protected void WritePalette()
		{
			fs.Write(colorTab, 0, colorTab.Length);
			int num = 768 - colorTab.Length;
			for (int i = 0; i < num; i++)
			{
				fs.WriteByte(0);
			}
		}

		protected void WritePixels()
		{
			new LZWEncoder(width, height, indexedPixels, colorDepth).Encode(fs);
		}

		protected void WriteShort(int value)
		{
			fs.WriteByte(Convert.ToByte(value & 0xFF));
			fs.WriteByte(Convert.ToByte((value >> 8) & 0xFF));
		}

		protected void WriteString(string s)
		{
			char[] array = s.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				fs.WriteByte((byte)array[i]);
			}
		}
	}
}
