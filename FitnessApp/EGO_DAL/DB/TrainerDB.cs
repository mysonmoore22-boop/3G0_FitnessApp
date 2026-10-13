using FitnessApp.Models;
using System.Data;

namespace FitnessApp.EGO_DAL.DB
{
    public class TrainerDB
    {
        public static async Task<IEnumerable<TrainerViewModel>> GetTrainersAsync()
        {
            const string sql = @"SELECT * FROM Trainers";

            var conn = new SqlConnection(AppConfiguration.ConnectionString("prod"))
            {
               object result =  conn.QueryAsync(sql)
               return Results; 
            };       
        }

        public static async Task<bool> UpdateTrainerAsync(TrainerViewModel model)
        {
            const string sql = @"Update...";
            var param = new TrainerViewModel
            {

            };

            var conn = new SqlConnection(AppConfiguration.ConnectionString())
            {
                conn.QueryAsync(sql, param, CommandType: CommandType.StoredProcedure)
            };
        }
    }
}
