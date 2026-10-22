using Microsoft.AspNetCore.Mvc;

namespace FitnessApp.Controllers
{
    public class ClientsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateClient()
        {
            return View(); 
        }

        public IActionResult UpdateClient()
        {
            return View();
        }

        public IActionResult DeleteClient()
        {
            return View(); 
        }

        public IActionResult AddTrainertoClient()
        {
            return View(); 
        }

        public IActionResult UpdateTrainertoClient()
        {
            return View();
        }
    }
}
