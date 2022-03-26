using System;
using System.Collections.Generic;
using System.Linq;

namespace SampSharp.Documentation.Services
{
	public class MenuTreeBuilder
	{
		private readonly List<Page> _pages = new List<Page>();
		private readonly List<Page> _folder = new List<Page>();

		public void AddPage(string path, string name, string routeName, object routeParams)
		{
			_pages.Add(new Page
			{
				Name = name,
				Path = path,
				RouteName = routeName,
				RouteParams =  routeParams
			});
		}

		public void AddFolder(string path, string name)
		{
			_folder.Add(new Page
			{
				Name = name,
				Path = path
			});
		}

		public TreeEntry[] Build()
		{
			var root = new TreeBuildEntry();
			foreach (var page in _pages)
			{
				var cur = root;
				foreach (var p in page.Path.Split('/', StringSplitOptions.RemoveEmptyEntries))
				{
					var next = cur.Entries.FirstOrDefault(c => c.UriPart == p);

					if (next == null)
					{
						var uri = cur.Uri == null ? p : cur.Uri + '/' + p;

						var folder = _folder.FirstOrDefault(f => f.Path == uri);

						next = new TreeBuildEntry
						{
							Name = folder.Name ?? p,
							UriPart = p,
							Uri = uri,
							RouteParams = page.RouteParams,
							RouteName = page.RouteName
						};
						cur.Entries.Add(next);
					}

					cur = next;
				}

				cur.Name = page.Name;
			}

			TreeEntry ConvertEntry(TreeBuildEntry e)
			{
				return e.Entries.Count == 0 ? new TreeEntry(e.Name, e.RouteName, e.RouteParams) : new TreeEntry(e.Name, null, e.Entries.Select(ConvertEntry).ToArray());
			}

			return root.Entries.Select(ConvertEntry).ToArray();
		}

		private class TreeBuildEntry
		{
			public string Name;
			public readonly List<TreeBuildEntry> Entries = new List<TreeBuildEntry>();
			public string UriPart;
			public string Uri;
			public string RouteName;
			public object RouteParams;
		}
		private struct Page
		{
			public string Path;
			public string Name;
			public string RouteName;
			public object RouteParams;
		}
	}
}