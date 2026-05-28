internal class UFPSCompilerHints
{
	public static bool CompilerHints()
	{
		vp_Message<int> obj = new vp_Message<int>("test");
		vp_Message<string, int> vp_Message2 = new vp_Message<string, int>("test");
		if (obj == null && vp_Message2 == null)
		{
			return true;
		}
		return true;
	}
}
