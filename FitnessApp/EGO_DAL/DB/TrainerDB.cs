using FitnessApp.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace FitnessApp.EGO_DAL.DB
{
    public class TrainerDB
    {
        public static async Task<IEnumerable<TrainerViewModel>> GetTrainersAsync()
        {
            const string sql = @"SELECT * FROM Trainers";

            using (var conn = new SqlConnection(AppConfiguration.GetConnectionString("prod")))
            {
                return await conn.QueryAsync<TrainerViewModel>(sql);
            }
        }

        public static async Task<bool> UpdateTrainerAsync(TrainerViewModel model)
        {
            const string sql = @"Update...";
            var param = new
            {
                // Map properties from model as needed, e.g.:
                // Id = model.Id,
                // Name = model.Name,
                // etc.
            };

            using (var conn = new SqlConnection(AppConfiguration.GetConnectionString("prod")))
            {
                var affectedRows = await conn.ExecuteAsync(sql, param, commandType: CommandType.StoredProcedure);
                return affectedRows > 0;
            }
        }
    }
}
