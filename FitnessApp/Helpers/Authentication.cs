using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;
using FitnessApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authentication; 
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Linq; 
using System.Security.Claims;
using System.Threading.Tasks;

namespace FitnessApp.Helpers
{
    public class Authentication : IdentityUser
    {

    };
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

    //public static class CustomeClaimTypes { }
    //    {
    //        public const string PositionID = "PositionID";
    //public const string TrainArea = "TrainArea";


    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly UserManager<IdentityUser> _userManager;
        
        public ClaimsTransformer(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true })
                return principal;

            // Can be invoked multiple times per request; only enrich once
            //if (principal.HasClaim(c => c.Type == t))
            //    return principal;

            var email = principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return principal;

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return principal;

            // Work on a copy rather than mutating the incoming principal
            var clone = principal.Clone();
            var identity = (ClaimsIdentity)clone.Identity!;

            // Roles (Admin / Trainer / Client)
            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                if (!identity.HasClaim(ClaimTypes.Role, role))
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            // Any custom claims stored on the user (e.g. subscription tier)
            foreach (var claim in await _userManager.GetClaimsAsync(user))
            {
                if (!identity.HasClaim(claim.Type, claim.Value))
                    identity.AddClaim(claim);
            }

            //identity.AddClaim(new Claim(TransformedMarker, "true"));
            return clone;
        }
    }
    public class UserCreation
    {
        private readonly UserManager<Authentication> _userManager;

        public UserCreation(UserManager<Authentication> userManager)
        {
            _userManager = userManager;
        }   

        public async Task<Authentication> CreateAspNetUserAccountAsync(string email, string password, bool isAuthenticated)
        {
            var user = new Authentication
            {
                UserName = email,
                Email = email,
                NormalizedEmail = email.ToUpper(),
                NormalizedUserName = email.ToUpper(),
                LockoutEnabled = isAuthenticated,
                PasswordHash = password
            };
            if(string.IsNullOrWhiteSpace(password))
            {
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception("User creation failed: "+ errorMessages);   
                }
            }
            else if(!string.IsNullOrWhiteSpace(password))
            {
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception("User creation failed: "+ errorMessages);   
                }
            }
            return await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
        }
    }
}
