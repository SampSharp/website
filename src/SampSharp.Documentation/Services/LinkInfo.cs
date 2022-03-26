using Microsoft.AspNetCore.Mvc;

namespace SampSharp.Documentation.Services
{
	public class LinkInfo
	{
		public LinkInfo(string name, string routeName, object routeParams)
		{
			Name = name;
			RouteName = routeName;
			RouteParams = routeParams;
		}

		public LinkInfo()
		{
		}

		public string Name { get; set; }
		public string RouteName { get; set; }
		public object RouteParams { get; set; }
		
		public string ToUrl(IUrlHelper url)
		{
			return url.RouteUrl(RouteName, RouteParams);
		}
	}
}