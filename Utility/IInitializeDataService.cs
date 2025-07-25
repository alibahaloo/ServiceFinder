using Microsoft.AspNetCore.Identity;
using ServiceFinder.Data;
using System.Collections;
using System.Globalization;
using System.Resources;

namespace ServiceFinder.Utility
{
    internal interface IInitializeDataService
    {
        Task InitializeBaseData(CancellationToken stoppingToken);
    }

    internal class InitializeRolesService : IInitializeDataService
    {
        private readonly ILogger<InitializeRolesService> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;

        private readonly ApplicationDbContext _context;

        public InitializeRolesService(
            ILogger<InitializeRolesService> logger,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IUserStore<ApplicationUser> userStore
            )
        {
            _roleManager = roleManager;
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _userStore = userStore;
            _emailStore = GetEmailStore();
        }

        private void CheckResourceFile()
        {
            bool error = false;

            ResourceManager MyResourceClass = new(typeof(InitResources));
            ResourceSet? resourceSet = MyResourceClass.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            if (resourceSet != null)
            {
                //Looping through all entries to make sure they're available
                foreach (DictionaryEntry entry in resourceSet)
                {
                    string? resourceKey = entry.Key.ToString();
                    object? resource = entry.Value;
                    if (resource != null && resource.ToString() == string.Empty || resource == null)
                    {
                        _logger.LogCritical($"CheckResourceFile - Missing Resource => {resourceKey}");
                        error = true;
                    }
                }
            }
            else
            {
                _logger.LogCritical($"CheckResourceFile - Missing Resource File: InitResources.resx");
                error = true;
            }

            if (error)
            {
                //Exit the program if resources are not available
                throw new ApplicationException("Errors with InitResources file");
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

        public async Task InitializeBaseData(CancellationToken stoppingToken)
        {
            CheckResourceFile();

            //Subscriptions
            char[] delimiter = new char[] { ',' };
            string[] baseSubscriptionNames = InitResources.Subscriptions.Split(delimiter);

            List<Subscription> baseSubscriptions = new();
            foreach (string subName in baseSubscriptionNames)
            {

                var sub = new Subscription { Name = subName, MaxMessages = 0, MaxServices = 0, Fee = 0 };
                baseSubscriptions.Add(sub);
            }

            foreach (var subscription in baseSubscriptions)
            {
                if (!_context.Subscriptions.Where(p => p.Name == subscription.Name).Any())
                {
                    _context.Subscriptions.Add(subscription);
                    await _context.SaveChangesAsync();
                }
            }

            //Province
            var baseProvince = new Province { Name = InitResources.Province };
            if (!_context.Provinces.Where(p => p.Name == baseProvince.Name).Any())
            {
                _context.Provinces.Add(baseProvince);
                await _context.SaveChangesAsync();
            }

            //City
            var baseCity = new City { Name = InitResources.City, ProvinceId = _context.Provinces.First().Id };
            if (!_context.Cities.Where(p => p.Name == baseCity.Name).Any())
            {
                _context.Cities.Add(baseCity);
                await _context.SaveChangesAsync();
            }

            //Industry
            var baseIndustry = new Industry { Name = InitResources.Industry };
            if (!_context.Industries.Where(p => p.Name == baseIndustry.Name).Any())
            {
                _context.Industries.Add(baseIndustry);
                await _context.SaveChangesAsync();
            }

            //Service Category
            var baseCategory = new Category { Name = InitResources.ServiceCategory, IndustryId = _context.Industries.First().Id };
            if (!_context.Categories.Any())
            {
                _context.Categories.Add(baseCategory);
                await _context.SaveChangesAsync();
            }

            //Roles
            var roles = UserAccountRoles.GetList();
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var newRole = new IdentityRole
                    {
                        Name = role
                    };
                    await _roleManager.CreateAsync(newRole);
                }
            }


            var AdminEmail = InitResources.AdminEmail;

            if (await _userManager.FindByEmailAsync(AdminEmail) == null)
            {
                var AdminPass = InitResources.AdminPassword;

                var user = Activator.CreateInstance<ApplicationUser>();
                user.UserType = UserAccountRoles.Admin;
                user.EmailConfirmed = true;
                user.IsVerified = true;
                user.FullName = InitResources.AdminFullName;
                user.AddressLine1 = InitResources.AdminAddress;
                user.PhoneNumber = InitResources.AdminPhoneNumber;
                user.CityId = _context.Cities.First().Id;
                user.PostalCode = InitResources.AdminPostalCode;
                user.SubscriptionId = _context.Subscriptions.First().Id; //Should be Premium (or whatever the last item in resx file is)

                await _userStore.SetUserNameAsync(user, AdminEmail, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, AdminEmail, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, AdminPass);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, UserAccountRoles.Admin);
                    _logger.LogInformation("Admin user created.");
                }

            }

            _logger.LogInformation("All base data added successfully.");
        }
    }
}
