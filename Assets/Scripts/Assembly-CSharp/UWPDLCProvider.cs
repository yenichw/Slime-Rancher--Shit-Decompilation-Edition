using System;
using System.Collections;
using System.Linq;
using DLCPackage;

public class UWPDLCProvider : DLCProvider
{
	public UWPDLCProvider()
		: base(Enumerable.Empty<Id>())
	{
	}

	public override IEnumerator Refresh()
	{
		yield return null;
	}

	public override void ShowInStore(Id id)
	{
		throw new NotImplementedException();
	}
}
