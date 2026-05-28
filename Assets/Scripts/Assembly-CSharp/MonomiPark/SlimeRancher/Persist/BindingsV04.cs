using System;
using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class BindingsV04 : VersionedPersistedDataSet<BindingsV03>
	{
		public class Binding
		{
			public string action;

			public int primKey;

			public int primMouse;

			public int secKey;

			public int secMouse;

			public int gamepad;

			public Binding()
			{
			}

			public Binding(string action, int primKey, int primMouse, int secKey, int secMouse, int gamepad)
			{
				this.action = action;
				this.primKey = primKey;
				this.primMouse = primMouse;
				this.secKey = secKey;
				this.secMouse = secMouse;
				this.gamepad = gamepad;
			}
		}

		public List<Binding> bindings = new List<Binding>();

		public override string Identifier => "SRBINDINGS";

		public override uint Version => 4u;

		public BindingsV04()
		{
		}

		public BindingsV04(BindingsV03 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				try
				{
					LoadNextBinding(reader);
				}
				catch (PersistedDataReadException)
				{
					throw;
				}
				catch (Exception innerException)
				{
					throw new PersistedDataReadException("Exception raised while reading key bindings.", innerException);
				}
			}
		}

		private void LoadNextBinding(BinaryReader reader)
		{
			ReadElementSeparator(reader);
			Binding binding = new Binding();
			binding.action = reader.ReadString();
			binding.primKey = reader.ReadInt32();
			binding.primMouse = reader.ReadInt32();
			binding.secKey = reader.ReadInt32();
			binding.secMouse = reader.ReadInt32();
			binding.gamepad = reader.ReadInt32();
			bindings.Add(binding);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(bindings.Count);
			foreach (Binding binding in bindings)
			{
				WriteBinding(binding, writer);
			}
		}

		private void WriteBinding(Binding binding, BinaryWriter writer)
		{
			WriteElementSeparator(writer);
			writer.Write(binding.action);
			writer.Write(binding.primKey);
			writer.Write(binding.primMouse);
			writer.Write(binding.secKey);
			writer.Write(binding.secMouse);
			writer.Write(binding.gamepad);
		}

		protected override void UpgradeFrom(BindingsV03 legacyData)
		{
		}
	}
}
