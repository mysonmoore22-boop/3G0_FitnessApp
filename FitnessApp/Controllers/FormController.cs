using Microsoft.AspNetCore.Mvc;

namespace FitnessApp.Controllers
{
    public class FormController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

    public class Task<IActionResult> Trainer(FitnessApp.Models.TrainerViewModelcs trainer)
    {
        var userID = trainer.UserID; try
        {

            //IN this module I will be loading the trainer data 








        }catch(Exception Ex)
        {
            return Ex; 
        }
    }
















}
