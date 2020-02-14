using System.Threading.Tasks;

namespace SampSharp.Documentation.Services
{
	public interface IViewRenderService
	{
		Task<string> RenderToStringAsync(string viewName, object model);
	}
}