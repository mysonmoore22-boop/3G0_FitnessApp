using FitnessApp.Models; 
namespace FitnessApp.EGO_BLL.BLL
{
    public class TrainerBLL
    {

        public static async Task<IEnumerable<TrainerViewModel>> GetTrainersAsync()
        {
            var result = EGO_DAL.DB.TrainerDB.GetTrainersAsync();
            return result; 
        }
        
        public static async Task<bool> UpdateTrainerAsync(Models.TrainerViewModel model)
        {
            var result = EGO_DAL.DB.TrainerDB.UpdateTrainerAsync(model);
            if(result != null)
            {
                return true; 
            }
            return false; 
        }
    }
}
