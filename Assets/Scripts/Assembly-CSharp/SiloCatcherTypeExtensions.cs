public static class SiloCatcherTypeExtensions
{
	public static bool HasInput(this SiloCatcher.Type type)
	{
		if (type != 0 && type != SiloCatcher.Type.REFINERY && type != SiloCatcher.Type.DECORIZER)
		{
			return type == SiloCatcher.Type.VIKTOR_STORAGE;
		}
		return true;
	}

	public static bool HasOutput(this SiloCatcher.Type type)
	{
		if (type != 0 && type != SiloCatcher.Type.SILO_OUTPUT_ONLY && type != SiloCatcher.Type.DECORIZER)
		{
			return type == SiloCatcher.Type.VIKTOR_STORAGE;
		}
		return true;
	}
}
