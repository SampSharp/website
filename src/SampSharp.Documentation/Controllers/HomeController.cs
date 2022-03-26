using Microsoft.AspNetCore.Mvc;
using SampSharp.Documentation.Models;
using SampSharp.Documentation.Repositories;

namespace SampSharp.Documentation.Controllers
{
    public class HomeController : SampSharpController
    {
        public HomeController(IVersionBuilder versionBuilder, IDataRepository dataRepository) :
            base(versionBuilder, dataRepository)
        {
        }
        
        [ResponseCache(Duration = 60 * 15, Location = ResponseCacheLocation.Any)]
        public IActionResult Index()
        {
            SetCurrentPage(Versions.First(v => v.IsDefault).Tag, SidebarName);
            return View(new HomeViewModel
            {
                Sidebar = Sidebar,
                VersionPicker = VersionPicker
            });
        }
    }
}