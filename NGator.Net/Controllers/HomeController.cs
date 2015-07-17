using System.Web.Mvc;

namespace NGator.Net.Controllers
{
    /// <summary>
    /// Default MVC5 controller
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Returns SPA Main page view
        /// </summary>        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "NGator.Net description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Garage71 contact page.";

            return View();
        }
    }
}