namespace FitnessApp.Models
{
    public static class AppConfiguration
    {
        static AppConfiguration()
        {
            FluentMapper.Initialize(x =>
            {
                x.AddMap(new TrainerViewModelMap());
            });
        }

        public static string GetConnectionString(string db)
        {
            switch (db.ToLower())
            {
                case "prod":
                    return "Server=localhost;Database=FitnessAppDB;User Id=sa;Password=YourStrong!Passw0rd;";
                case "dev":
                    return "Server=localhost;Database=FitnessAppDB_Dev;User Id=sa;Password=YourStrong!Passw0rd;";
                case "test":
                    return "Server=localhost;Database=FitnessAppDB_Test;User Id=sa;Password=YourStrong!Passw0rd;";  
                default:
                    throw new ArgumentException("Invalid database name");
            }
        }

        internal static object GetConnectionString()
        {
            throw new NotImplementedException();
        }
    }
}
