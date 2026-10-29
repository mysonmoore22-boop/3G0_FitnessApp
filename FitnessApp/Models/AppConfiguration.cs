using Dapper.FluentMap;

namespace FitnessApp.Models
{
    public static class AppConfiguration
    {
        static AppConfiguration()
        {
            FluentMapper.Initialize(x =>
            {
                //x.AddMap(new TrainerViewModelMap());
            });
        }

        public static string GetConnectionString(string db)
        {
            switch (db.ToLower())
            {
                case "prod":
                    return "Data Source=localhost;Initial Catalog=CorporateDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Application Name=FitnessApp";
                case "dev":
                    return "Server=localhost;Database=FitnessAppDB_Dev;User Id=sa;Password=YourStrong!Passw0rd;";
                case "test":
                    return "Server=localhost;Database=FitnessAppDB_Test;User Id=sa;Password=YourStrong!Passw0rd;";  
                default:
                    throw new ArgumentException("Invalid database name");
            }
        }

        internal static object Initialize(ConfigurationManager configuration)
        {
           return null;
        }

        //internal static void Initialize(ConfigurationManager configuration)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
