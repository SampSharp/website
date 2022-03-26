namespace SampSharp.Documentation.Services
{
	public class TreeEntry
	{
		public TreeEntry()
		{
		}

		public TreeEntry(string name, TreeEntry[] entries)
		{
			Link = new LinkInfo(name, null, null);
			Entries = entries;
		}

		public TreeEntry(string name, string routeName, object routeParams)
		{
			Link = new LinkInfo(name, routeName, routeParams);
		}

		public TreeEntry[] Entries { get; set; }
		public LinkInfo Link { get; set; }
	}
}