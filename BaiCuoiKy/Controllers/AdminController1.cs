using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaiCuoiKy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}


//admin @gmail.com
//Admin@123