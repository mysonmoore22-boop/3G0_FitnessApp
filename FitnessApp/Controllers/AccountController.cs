using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace FitnessApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _Config;
        private readonly ILogger<AccountController> _Logger;
        //private readonly IRecatpchaVerifier _recaptcha;
        private readonly RoleManager<IdentifyRole> _roleManager;
        private readonly IRoleService _roleService;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<Authentication> _userManager;
        private readonly userService _userService;
        private readonly IWebHostEnvironment _env; 
        private readonly string? Email;
        private readonly string? user; 



        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLogin(string? email, string provider, string returnUrl = null)
        {
            TempData["Email"] = email;
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl = returnUrl ?? "/" });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, provider); //redirects to goole authentication. 
        }

        public async Task<IActionResult> ExternalLoginCallback(string? email, string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/"); 
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _Logger.LogWarning("External Login information is null. Cookies: " + string.Join(", ", HttpContext.Request.Cookies.Keys));
                return RedirectToAction(nameof(Login));
            }

            foreach(var claim in info.Principal.Claims)
            {
                _Logger.LogInformation($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
            }
            try
            {

                email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if(string.IsNullOrEmpty(email))
                {
                    ModelState.AddModelError(string.Empty, "Unable to determine email. Please Register"); 
                    return RedirectToAction(nameof(Login));
                }

                // Sign in the user with this external login provider if the user already has a login.
                var result = await _userService.ProcessExternalLogin(info, email, returnUrl); 
                if (result.Succeeded)
                {
                    // Update any authentication tokens
                    await _signInManager.SignInAsync(result.User, isPersistent: false);

                    var (userEmail, roleName) = await _roleService.GetRoleAssignmentAsync(result.User);
                    var Email = userEmail; 
                    if(!string.IsNullOrEmpty(roleName))
                    {
                        _Logger.LogInformation($"User {Email} has role: {roleName}");
                        var principal = await _signInManager.CreateUserPrincipalAsync(result.User);
                        return _roleService.RedirectToDashBoard(principal, roleName);
                    }
                    else
                    {
                        _Logger.LogInformation($"User {Email} has no assigned role.");
                        return RedirectToAction(nameof(Login));
                    })
                    return LocalRedirect(returnUrl ?? "/");
                }
                else
                {
                    // If the user does not have an account, then ask the user to create an account.
                    ViewData["ReturnUrl"] = returnUrl;
                    ViewData["LoginProvider"] = info.LoginProvider;
                    email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
                    return View("ExternalLoginConfirmation", new Models.ExternalLoginConfirmationViewModel { Email = email });
                }
            }catch(UnauthorizedAccessException)
            {
                return RedirectToAction(nameof(Login));
            }
           return LocalRedirect(returnUrl ?? "/");
        }

        public async Task<IActionResult> Login()
        {
            //The View for the login page is returned here. 
            try { }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Optionally, you can return an error view or message
                return View("Error");
            }
            return View();
        }

        [ResponseCache(NoStore =true, Location = ResponseCacheLocation.None)]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(string? returnUrl = "/")
        {
            //The View for the logout page is returned here. 

            try
            {
                await HttpContext.SignOutAsync();
                await _signInManager.SignOutAsync();

                Debug.WriteLine("User Authenticated?" + User.Identity.IsAuthenticated);
                TempData["SuccessMessage"] = "You have been logged out successfully.";
                return RedirectToAction("LoggedOUT", "Account");   
            }catch(Exception ex)
            {
                // Log the exception or handle it as needed
                TempData["ErrorMessage"] = "An error occurred during logout. Please try again.";
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Optionally, you can return an error view or message
                return View("Error");
            }
        }

        public async Task<IActionResult> LoggedOUT()
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
        }

        public async Task<IActionResult> LoggedIn(string? recaptchaToken, string? error, string? returnUrl = "/", string? email = "", string? password = "")
        {
            var token = recaptchaToken;
            var (ok, reason, raw) = await _recaptcha.VerfiyV3Async(token, expectedAction: "login", expectedHostname: Request.Host.Host);

            if (ok) {
                TempData["ErrorMessage"] = reason ?? "reCaptcha failed.";
                return View("Login");
            }

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

                    if (await _userMnager.IsLockedOutAsync(user))
                    {
                        var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                        TempData["ErrorMessage"] = $"Your account is locked until {lockoutEnd?.LocalDateTime}.";
                        return RedirectToAction("Login", "Account");
                    }

                    var verify = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

                    if (verify == passwordVerificationResult.Success || verify = passwordVerificationResult.SuccessRehashNeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Home", "Dashboard");
                    }
                    await _userManager.AccessFailedAsync(user);

                    int count = await _UserManager.GetAccessFailedCountAsync(user);
                    int max = _userManager.Options.Lockout.MaxFailedAccessAttempts;
                    int remcount = max - count;

                    if (count >= max)
                    {
                        TempData["ErrorMessage"] = "Account Locked, too many attempts.";
                        return RedirectToAction("Login", "Account");
                    }

                    TempData["ErrorMessage"] = $"Incorrect username or password. <br />Attempts remaining:{" " + remcount}";
                    return RedirectToAction("Login", "Account");
                } catch (ArgumentNullException ex)
                {
                    TempData["ErrorMessage"] = "Add a password to your account.";
                    return RedirectToAction("Login", "Account");
                } catch (Exception ex)
                {
                    _Logger.LogError(ex, "An error occurred during login.");
                    TempData["Exception"] = ex.Message
                    return RedirectToAction("Index", "Home");
                }
                else {

                    if (string.IsNullOrEmpty(error) || error == "unauthorized")
                    {
                        TempData["ErrorMessage"] = "No user account associated with that email. Please register.";
                        return RedirectToAction("Login", "Account");
                    }
                }
                RedirectToAction("Home", "Dashboard");
            }
        }

        public static async Task<IActionResult> Register()
                    {
                        //The View for the register page is returned here. 
                        try { }
                        catch (Exception ex)
                        {
                            // Log the exception or handle it as needed
                            Console.WriteLine($"An error occurred: {ex.Message}");
                            // Optionally, you can return an error view or message
                            return new ViewResult { ViewName = "Error" };
                        }
                        return new ViewResult { ViewName = "Register" };
                    }

        //public static async Task<IActionResult> UpdateTrainer()
        //{
        //}

        //public static async Task<IActionResult> SubmitTrainer()
        //{

        //}

        public async Task<IActionResult> AccessDenied(string? returnUrl = null)
        {
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            _Logger.LogWarning("Access denied to {Url} for {User}. Roles in cookie: {Roles}", returnUrl, User.Identity?.Name, string.Join(", ", roles));
            return View(); 
        }

        public static async Task<IActionResult> ForgotPassword()
        {
            //The View for the forgot password page is returned here. 
            try { }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Optionally, you can return an error view or message
                return new ViewResult { ViewName = "Error" };
            }
            return new ViewResult { ViewName = "ForgotPassword" };
        }

        public static async Task<IActionResult> LoggedIn()
        {
                       //The View for the logged in page is returned here. 
            try { }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Optionally, you can return an error view or message
                return new ViewResult { ViewName = "Error" };
            }
            return new ViewResult { ViewName = "LoggedIn" };
        }

    }
}
