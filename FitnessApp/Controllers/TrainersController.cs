using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Composition;

namespace FitnessApp.Controllers
{
    public class TrainersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateTrainer()
        {
            return View();
        }

        public IActionResult UpdateTrainer()
        {
            return View();
        }

        public IActionResult DeleteTrainer()
        {
            return View();
        }

        public IActionResult Clients()
        {
            return View();
        }

        public IActionResult ClientDetails()
        {
            return View();
        }

        public IActionResult RemoveClient()
        {
            return View(); 
        }
    }
}
