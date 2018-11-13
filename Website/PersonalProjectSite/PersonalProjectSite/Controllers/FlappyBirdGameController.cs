using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PersonalProjectSite.Controllers
{
    public class FlappyBirdGameController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}