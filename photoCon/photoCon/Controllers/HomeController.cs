using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using photoCon.Dto;
using photoCon.Models;
using System.Diagnostics;
using System.Security.Claims;
using photoCon.Interface;

namespace photoCon.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountManagementRepository _accountManagementRepository;

        public HomeController(
                                ILogger<HomeController> logger
                                ,SignInManager<ApplicationUser> signInManager
                                ,UserManager<ApplicationUser> userManager
                                ,IAccountManagementRepository enrollmentRepository
                                ,IAuditLogsRepository auditLogsRepository
                            )
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;

            _accountManagementRepository = enrollmentRepository;
        }
        public IActionResult Login()
        {
            _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AuthenticateUser([FromBody] LoginView model)
        {
            var rolename_ = "";
            var jsonData = new ServerResponseGeneric
            {
                ServerResponseCode = 0,
                ServerResponseMessage = "",
            };

            try
            {

                if (!ModelState.IsValid)
                {
                    jsonData.ServerResponseCode = 410;
                    jsonData.ServerResponseMessage = "Invalid login attempt.";
                    return Ok(JsonConvert.SerializeObject(jsonData));
                }

                bool isSystemUser = false;
                if (_accountManagementRepository.GetUsersList().Where(a => a.UserName == model.Username).Count() > 0)
                {
                    isSystemUser = true;
                }

                var PAGCORID_Verify = _accountManagementRepository.VerifyInternalUser(model.Username, model.Password);
                bool PAGCORID_isValid = (_accountManagementRepository.GetInternalUserInfo(model.Username).Where(a => a.UserName == model.Username).Count() > 0);
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                bool isAuthenticated = false; 

                if (PAGCORID_isValid)
                {
                    if (PAGCORID_Verify == true && isSystemUser == true)
                    { //-->System User AND PAGCOR Employee
                        isAuthenticated = true;
                    }
                    else
                    {
                        isAuthenticated = false;
                    }
                }
                else
                {
                    if (isSystemUser == true && result.Succeeded)
                    { //--> System User with Correct Password
                        isAuthenticated = true;
                    }
                    else
                    {
                        isAuthenticated = false;
                    }
                }

                if (isAuthenticated)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);

                    if (user != null)
                    {
                        // Update last login timestamp
                        user.LastLoginDateTime = DateTime.UtcNow;

                        // Update security stamp
                        await _userManager.UpdateSecurityStampAsync(user);

                        // Reset access failed count
                        await _userManager.ResetAccessFailedCountAsync(user);

                        // Log successful login event
                        _logger.LogInformation("User {Username} logged in at {LoginTime}.", model.Username, DateTime.UtcNow);

                        var roles = await _userManager.GetRolesAsync(user);
                        var roleName = roles.FirstOrDefault();
                        rolename_ = roleName;
                        

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.GivenName, user.FirstName),
                            new Claim(ClaimTypes.Surname, user.LastName),

                            // Add user ID claim
                            new Claim(ClaimTypes.NameIdentifier, user.Id),
                            // Add role claim if the user has a role
                            !string.IsNullOrEmpty(roleName) ? new Claim(ClaimTypes.Role, roleName) : null
                            // Add more claims as needed
                        };




                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            // Configure authentication properties as needed
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Example: Set cookie expiration
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    }

                    var jsonReturn = new
                    {
                        ServerResponseCode = 200,
                        ServerResponseMessage = "Operation Successful",
                        //UpdatedOrInsertedData = rolename_ + model.Username,
                    };

                    return Ok(JsonConvert.SerializeObject(jsonReturn));
                }
                else if (result.IsLockedOut)
                {
                    // User account is locked out
                    // Handle accordingly
                    jsonData.ServerResponseCode = 422; // Custom status code for account lockout
                    jsonData.ServerResponseMessage = "Your account is locked out. Please contact support.";
                    return Ok(JsonConvert.SerializeObject(jsonData));
                }
                else if (result.IsNotAllowed)
                {
                    // User is not allowed to sign in (e.g., email not confirmed)
                    // Handle accordingly
                    jsonData.ServerResponseCode = 423; // Custom status code for not allowed
                    jsonData.ServerResponseMessage = "Your account is not allowed to sign in. Please check your email for verification instructions.";
                    return Ok(JsonConvert.SerializeObject(jsonData));
                }
                else
                {
                    // Authentication failed due to invalid username or password
                    // Determine if the username exists
                    var user = await _userManager.FindByNameAsync(model.Username);
                    if (user != null)
                    {
                        // Username exists but password is wrong
                        jsonData.ServerResponseCode = 421; // Custom status code for wrong password
                        jsonData.ServerResponseMessage = "Invalid password. Please try again.";
                        return Ok(JsonConvert.SerializeObject(jsonData));
                    }
                    else
                    {
                        // Username does not exist
                        jsonData.ServerResponseCode = 424; // Custom status code for username not found
                        jsonData.ServerResponseMessage = "Username does not exist.";
                        return Ok(JsonConvert.SerializeObject(jsonData));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                jsonData.ServerResponseCode = 500;
                //jsonData.ServerResponseMessage = $"An error occurred: {ex.Message}";
                jsonData.ServerResponseMessage = $"An error occurred: Contact System Administrator.";
                return Ok(JsonConvert.SerializeObject(jsonData));
            }
        }

        [Authorize]
        public async Task<IActionResult> LoginSuccess()
        {
            // Get the username of the currently authenticated user
            var username = User.Identity.Name;

            // Get the user object
            var user = await _userManager.FindByNameAsync(username);

            // Get the roles of the user
            var roles = await _userManager.GetRolesAsync(user);

            // Check the roles and redirect accordingly
            if (roles.Contains("SYSADMIN") || roles.Contains("APPADMIN") || roles.Contains("ECDUSER"))
            {
                // Redirect to home/index
                return RedirectToAction("Index", "Home");
            }
            else if (roles.Contains("JUDGE"))
            {
                // Redirect to judge/index
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Handle other roles or scenarios if needed
                // Redirect to a default page or display an error
                return RedirectToAction("Index", "Default");
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }



        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "SYSADMIN")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}