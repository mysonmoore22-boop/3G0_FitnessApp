using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;

namespace FitnessApp.Helpers
{
    public class Authentication : IdentityUser
    {

        public class LoginResult
        {
            public bool IsAuthenticated { get; set; }
            public string? trainermatch { get; set; }
            public string? clientmatch { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public string Username { get; set; }
            public bool IsEmailConfirmed { get; set; }
            public int? TrainerID { get; set; }
            public int? ClientID { get; set; }
        }

        public static class Roles
        {
            public const string Client = "client";
            public const string Trainer = "trainer";
            public const string Owner = "owner";
            public const string SuperUser = "superuser";
        }

        public static class CustomeClaimTypes { }
        {
            public const string PositionID = "PositionID";
            public const string TrainArea = "TrainArea";
        };
        









    }
}
