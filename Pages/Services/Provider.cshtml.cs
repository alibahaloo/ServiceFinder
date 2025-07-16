using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Services
{
    [Authorize]
    public class ProviderModel : BasePageModel
    {
        private readonly ServiceRepo _serviceRepo;
        private readonly UserRepo _userRepo;
        public ProviderModel(ServiceRepo serviceRepo, IFlashMessage flashMessage, UserManager<Data.ApplicationUser> userManager, UserRepo userRepo)
            : base(userManager, flashMessage)
        {
            _serviceRepo = serviceRepo;
            _userRepo = userRepo;
        }
        public Data.ApplicationUser ServiceProvider { get; set; } = default!;
        public IList<Data.Service> Services { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userRepo.GetEntityAsync(id);
            if (user == null || user.UserType != UserAccountRoles.ServiceProvider)
            {
                return NotFound();
            }

            ServiceProvider = user;
            Services = await _serviceRepo.GetProviderServicesAsync(id);


            return Page();
        }
    }
}
