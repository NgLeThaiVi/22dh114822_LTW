using _22dh114822_LTW.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _22dh114822_LTW.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        // GET: Admin/Home
        public ActionResult Index()
        {
            var model = new HomeProductVM(); // Phải có
            return View();
        }
    }
}