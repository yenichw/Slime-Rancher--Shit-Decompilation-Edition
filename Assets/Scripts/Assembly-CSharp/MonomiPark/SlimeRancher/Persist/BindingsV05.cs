using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonomiPark.SlimeRancher.Persist
{
	public class BindingsV05 : VersionedPersistedDataSet<BindingsV04>
	{
		public List<BindingV01> bindings = new List<BindingV01>();

		public override string Identifier => "SRBINDINGS";

		public override uint Version => 5u;

		public BindingsV05()
		{
		}

		public BindingsV05(BindingsV04 previous)
		{
			UpgradeFrom(previous);
		}

		protected override void LoadData(BinaryReader reader)
		{
			bindings = PersistedDataSet.LoadList<BindingV01>(reader);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WriteList(writer, bindings);
		}

		protected override void UpgradeFrom(BindingsV04 previous)
		{
			bindings = previous.bindings.Select((BindingsV04.Binding b) => new BindingV01
			{
				action = b.action,
				primKey = b.primKey,
				primMouse = b.primMouse,
				secKey = b.secKey,
				secMouse = b.secMouse,
				gamepad = b.gamepad
			}).ToList();
			BindingV01 bindingV = bindings.FirstOrDefault((BindingV01 b) => b.action == "OpenMap");
			if (bindingV != null)
			{
				bindings.Add(new BindingV01
				{
					action = "CloseMap",
					primKey = bindingV.primKey,
					primMouse = bindingV.primMouse,
					secKey = bindingV.secKey,
					secMouse = bindingV.secMouse,
					gamepad = bindingV.gamepad
				});
			}
		}

		public static void AssertAreEqual(BindingsV04 expected, BindingsV05 actual)
		{
			TestUtil.AssertAreEqual(expected.bindings, actual.bindings, delegate(BindingsV04.Binding e, BindingV01 a, string s)
			{
				BindingV01.AssertAreEqual(e, a);
			});
		}

		public static void AssertAreEqual(BindingsV05 expected, BindingsV05 actual)
		{
			TestUtil.AssertAreEqual(expected.bindings, actual.bindings, delegate(BindingV01 e, BindingV01 a, string s)
			{
				BindingV01.AssertAreEqual(e, a);
			});
		}
	}
}
