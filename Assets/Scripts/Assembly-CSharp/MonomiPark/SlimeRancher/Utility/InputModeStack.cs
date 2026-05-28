using System;
using System.Collections.Generic;
using System.Linq;

namespace MonomiPark.SlimeRancher.Utility
{
	public class InputModeStack
	{
		private class Entry
		{
			public int handle;

			public SRInput.InputMode inputMode;
		}

		private List<Entry> entryStack = new List<Entry>();

		public bool Push(SRInput.InputMode mode, int handle)
		{
			if (!entryStack.Exists((Entry entry) => entry.handle == handle))
			{
				entryStack.Add(new Entry
				{
					handle = handle,
					inputMode = mode
				});
				return true;
			}
			return false;
		}

		public void Pop(int handle)
		{
			entryStack.RemoveAll((Entry entry) => entry.handle == handle);
		}

		public SRInput.InputMode Peek()
		{
			if (entryStack.Count == 0)
			{
				throw new Exception("Cannot peek at empty InputModeStack!");
			}
			return entryStack.Last().inputMode;
		}
	}
}
