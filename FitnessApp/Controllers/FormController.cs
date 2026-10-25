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
    
    //[HttpGet]
    //public class Task<IActionResult> Trainer(FitnessApp.Models.TrainerViewModel trainer)
    //{
    //    var userID = trainer.UserID; 
    //    try
    //    {

    //        //IN this module I will be loading the trainer data 



    //    }catch(Exception Ex)
    //    {
    //        return Ex; 
    //    }
    //}
    //[ValidateAntiForgeryToken]
    //[HttpPost]
    //public class Task<IActionResult> _UpdateTrainer([FromForm] )
    //{
    //    try
    //    {
    //        var task = EGO_DAL.DB.TrainerDB
    //    }
    //    catch (Exception Ex) 
    //    {
    //        return HttpRequestError.InvalidResponse;
    //    }

    //    return HttpRequestError.ConnectionError;
    //}
















}
