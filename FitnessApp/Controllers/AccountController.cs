using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FitnessApp.Models;
//using FitnessApp.Services;
using FitnessApp.Helpers;
using System.Diagnostics;
namespace FitnessApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _Config;
        private readonly ILogger<AccountController> _Logger;
        //private readonly IRecatpchaVerifier _recaptcha;
        private readonly RoleManager<Authentication> _roleManager;
        //private readonly IRoleService _roleService;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<Authentication> _userManager;
        //private readonly userService _userService;
        private readonly IWebHostEnvironment _env; 
        private readonly string? Email;
        private readonly string? user;

        public IActionResult Index()
        {
            return View();
        }

        //[AllowAnonymous]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult ExternalLogin(string? email, string provider, string? returnUrl = null)
        //{
        //    if (!Url.IsLocalUrl(returnUrl))
        //        returnUrl = "/";

        //    TempData["Email"] = email;

        //    var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        //    var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        //    return Challenge(properties, provider); // redirects to Google
        //}
        //[AllowAnonymous]
        //public async void ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        //{
        //    if (!Url.IsLocalUrl(returnUrl)) returnUrl = Url.Content("~/");

        //    if (remoteError != null)
        //    {
        //        TempData["Error"] = $"Error from external provider: {remoteError}";
        //         RedirectToAction(nameof(Login));
        //    }

        //    var info = await _signInManager.GetExternalLoginInfoAsync();
        //    if (info == null)
        //    {
        //        _Logger.LogWarning("External login info was null.");
        //         RedirectToAction(nameof(Login));
        //    }

        //    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        //    if (string.IsNullOrEmpty(email))
        //    {
        //        TempData["Error"] = "Unable to determine email. Please register.";
        //        RedirectToAction(nameof(Login));
        //    }

        //    try
        //    {
        //        //var result = await _userService.ProcessExternalLogin(info, email, returnUrl);
        //        //if (!result.Succeeded)
        //        //{
        //        //    ViewData["ReturnUrl"] = returnUrl;
        //        //    ViewData["LoginProvider"] = info.LoginProvider;
        //        //    RedirectToAction(nameof(View("ExternalLoginConfirmation",
        //        //        new Models.ExternalLoginConfirmationViewModel { Email = email });
        //        //}

        //        await _signInManager.SignInAsync(result.User, isPersistent: false);
        //        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        //        //var (userEmail, roleName) = await _roleService.GetRoleAssignmentAsync(result.User);
        //        //if (string.IsNullOrEmpty(roleName))
        //        //{
        //        //    _Logger.LogInformation("User {Email} has no assigned role.", userEmail);
        //        //    await _signInManager.SignOutAsync();
        //        //    TempData["Error"] = "Your account has no assigned role.";
        //        //    RedirectToAction(nameof(Login));
        //        //}

        //        _Logger.LogInformation("User {Email} has role: {Role}", userEmail, roleName);
        //        var principal = await _signInManager.CreateUserPrincipalAsync(result.User);
        //        //_roleService.RedirectToDashBoard(principal, roleName);
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        RedirectToAction(nameof(Login));
        //    }
        //}
        [AllowAnonymous]
        public IActionResult Login()
        {
            //The View for the login page is returned here. 
            try {
                //var user = await HttpContext.AuthenticateAsync();

                return View();
            }
            catch (Exception ex)
            {
                 // Log the exception or handle it as needed
                 Console.WriteLine($"An error occurred: {ex.Message}");
                 // Optionally, you can return an error view or message
                 return View("Error");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async void Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                TempData["SuccessMessage"] = "You have been logged out successfully.";
                RedirectToAction(nameof(LoggedOUT), "Account");
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error during logout");
                RedirectToAction (nameof(ErrorEventArgs));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoggedOUT()
        {
            //The view for the logged out page is returned here. 
            try
            {
                TempData["SuccessMessage"] = "Account logged out successfully.";
                return RedirectToAction("Login", "Account");
            }
            catch(Exception Ex)
            {
                return View("Error");  
            }
            return null; 
        }

        public async void LoggedIn(string? recaptchaToken, string? error, string? returnUrl = "/", string? email = "", string? password = "")
        {
            var token = recaptchaToken;
            //var (ok, reason, raw) = await _recaptcha.VerfiyV3Async(token, expectedAction: "login", expectedHostname: Request.Host.Host);

            //if (ok) {
            //    TempData["ErrorMessage"] = reason ?? "reCaptcha failed.";
            //    RedirectToAction(nameof(Login));
            //}

            TempData["Email"] = email;

            var passwordHasher = new PasswordHasher<Authentication>();
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                try
                {
                    if (!await _userManager.GetLockoutEnabledAsync(user))
                    {
                        await _userManager.SetLockoutEnabledAsync(user, true);
                    }

                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                        TempData["ErrorMessage"] = $"Your account is locked until {lockoutEnd?.LocalDateTime}.";
                        RedirectToAction("Login", "Account");
                        return;
                    }

                    var verify = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

                    //if (verify == passwordVerificationResult.Success || verify = passwordVerificationResult.SuccessRehashNeeded)
                    //{
                    //    await _userManager.ResetAccessFailedCountAsync(user);
                    //    await _signInManager.SignInAsync(user, isPersistent: false);
                    //    return RedirectToAction("Home", "Dashboard");
                    //}
                    await _userManager.AccessFailedAsync(user);

                    int count = await _userManager.GetAccessFailedCountAsync(user);
                    int max = _userManager.Options.Lockout.MaxFailedAccessAttempts;
                    int remcount = max - count;

                    if (count >= max)
                    {
                        TempData["ErrorMessage"] = "Account Locked, too many attempts.";
                        RedirectToAction("Login", "Account");
                        return;
                    }

                    TempData["ErrorMessage"] = $"Incorrect username or password. <br />Attempts remaining:{" " + remcount}";
                    RedirectToAction("Login", "Account");
                    return;
                }
                catch (ArgumentNullException ex)
                {
                    TempData["ErrorMessage"] = "Add a password to your account.";
                    RedirectToAction("Login", "Account");
                    return;
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex, "An error occurred during login.");
                    TempData["Exception"] = ex.Message;
                    RedirectToAction("Index", "Home");
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(error) || error == "unauthorized")
                {
                    TempData["ErrorMessage"] = "No user account associated with that email. Please register.";
                    RedirectToAction("Login", "Account");
                    return;
                }
                RedirectToAction("Home", "Dashboard");
            }
        }

        //public static async Task<IActionResult> Register()
        //{
        //    //The View for the register page is returned here. 
        //    try { }
        //    catch (Exception ex)
        //    {
        //        // Log the exception or handle it as needed
        //        Console.WriteLine($"An error occurred: {ex.Message}");
        //        // Optionally, you can return an error view or message
        //        return new ViewResult { ViewName = "Error" };
        //    }
        //    return new ViewResult { ViewName = "Register" };
        //}

        //public static async Task<IActionResult> UpdateTrainer()
        //{
        //}

        //public static async Task<IActionResult> SubmitTrainer()
        //{

        //}

        //public async Task<IActionResult> AccessDenied(string? returnUrl = null)
        //{
        //    var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        //    _Logger.LogWarning("Access denied to {Url} for {User}. Roles in cookie: {Roles}", returnUrl, User.Identity?.Name, string.Join(", ", roles));
        //    return View(); 
        //}

        //public static async Task<IActionResult> ForgotPassword()
        //{
        //    //The View for the forgot password page is returned here. 
        //    try { }
        //    catch (Exception ex)
        //    {
        //        // Log the exception or handle it as needed
        //        Console.WriteLine($"An error occurred: {ex.Message}");
        //        // Optionally, you can return an error view or message
        //        return new ViewResult { ViewName = "Error" };
        //    }
        //    return new ViewResult { ViewName = "ForgotPassword" };
        //}

        //public static Task<IActionResult> LoggedIn(Task task)
        //{
        //    //The View for the logged in page is returned here. 
        //    try { }
        //    catch (Exception ex)
        //    {
        //        // Log the exception or handle it as needed
        //        Console.WriteLine($"An error occurred: {ex.Message}");
        //        // Optionally, you can return an error view or message
        //        return Task.FromResult<IActionResult>(new ViewResult { ViewName = "Error" });
        //    }
        //    return Task.FromResult<IActionResult>(new ViewResult { ViewName = "LoggedIn" });
        //}

    }
}
