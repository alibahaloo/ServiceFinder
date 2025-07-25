// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using ServiceFinder.Utility;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace ServiceFinder.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        private readonly ServiceFinder.Data.ApplicationDbContext _context;
        private readonly SubscriptionRepo _subscriptionRepo;
        private readonly EmailService _emailService;
        private readonly InvitationRepo _invitationRepo;

        [BindProperty]
        public string Type { get; set; }

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            SubscriptionRepo subscriptionRepo,
            EmailService emailService,
            InvitationRepo invitationRepo)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _roleManager = roleManager;
            _subscriptionRepo = subscriptionRepo;
            _emailService = emailService;
            _invitationRepo = invitationRepo;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "Full Name")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            public string FullName { get; set; }

            [Required]
            [Display(Name = "Address Line 1")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }

            [Required]
            [Display(Name = "Postal Code")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.PostalCode)]
            public string PostalCode { get; set; }

            [Required]
            [Display(Name = "City")]
            public int CityId { get; set; }

            [Required]
            [Display(Name = "Phone Number")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 7)]
            [DataType(DataType.PhoneNumber)]
            public string PhoneNumber { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null, string type = null, string email = null)
        {
            if (type != null) { Type = type; }
            if (email != null) { Input = new InputModel { Email = email }; }

            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name");
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            //returnUrl ??= Url.Content("~/");
            switch (Type)
            {
                case UserAccountRoles.Customer:
                    returnUrl = Url.Content("~/Services");
                    break;
                case UserAccountRoles.ServiceProvider:
                    returnUrl = Url.Content("~/MyServices");
                    break;
            }
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = await CreateUserAsync();

                user.FullName = Input.FullName;
                user.AddressLine1 = Input.AddressLine1;
                user.AddressLine2 = Input.AddressLine2;
                user.PhoneNumber = Input.PhoneNumber;
                user.CityId = Input.CityId;
                user.PostalCode = Input.PostalCode.ToUpper();
                user.JoinedOn = DateTime.UtcNow;

                //Add free subscription by default for all newly registered users
                var freeSubscription = await _subscriptionRepo.GetFreeSubscriptionAsync();
                user.SubscriptionId = freeSubscription.Id;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    //Send confirmation email
                    _emailService.SendConfirmation(Input.Email, HtmlEncoder.Default.Encode(callbackUrl), Type);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private async Task<ApplicationUser> CreateUserAsync()
        {

            //Check if roles exists
            if (!await _roleManager.RoleExistsAsync(Type))
            {
                //If not, create before registering the user,
                var role = new IdentityRole
                {
                    Name = Type
                };
                await _roleManager.CreateAsync(role);
            }

            ApplicationUser user = new();
            user = Activator.CreateInstance<ApplicationUser>();
            try
            {
                //Check for the given Type, and register the user accordingly
                switch (Type)
                {
                    case UserAccountRoles.Customer:
                        user.UserType = UserAccountRoles.Customer;
                        await _userManager.AddToRoleAsync(user, UserAccountRoles.Customer);
                        break;
                    case UserAccountRoles.ServiceProvider:
                        user.UserType = UserAccountRoles.ServiceProvider;
                        await _userManager.AddToRoleAsync(user, UserAccountRoles.ServiceProvider);
                        break;
                    default:
                        throw new InvalidOperationException("A user type must be selected before proceeding to create a user");
                }

                return user;
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
