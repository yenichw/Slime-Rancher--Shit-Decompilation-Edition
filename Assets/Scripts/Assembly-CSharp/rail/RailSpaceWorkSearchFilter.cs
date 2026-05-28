using System.Collections.Generic;

namespace rail
{
	public class RailSpaceWorkSearchFilter
	{
		public List<string> excluded_tags = new List<string>();

		public List<string> required_tags = new List<string>();

		public string search_text;
	}
}
