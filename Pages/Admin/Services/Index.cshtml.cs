using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;

namespace ServiceFinder.Pages.Admin.Services
{
    public class IndexModel : AdminPageModel
    {
        private readonly ServiceRepo _serviceRepo;
        private readonly UserRepo _userRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ServiceRepo serviceRepo, UserRepo userRepo, UserManager<ApplicationUser> userManager)
        {
            _serviceRepo = serviceRepo;
            _userRepo = userRepo;
            _userManager = userManager;
        }

        public IList<Service> Services { get; set; } = default!;
        public ApplicationUser? ServiceProvider { get; set; } = default!;
        public int CurPage { get; set; }
        public int PageCount { get; set; }
        public async Task<IActionResult> OnGetAsync(string providerId, int curPage = 1)
        {

            //check if the provider is an actual user
            var provider = await _userManager.FindByIdAsync(providerId);

            if (provider == null) { return BadRequest(); }

            if (provider.UserType != UserAccountRoles.ServiceProvider) { return BadRequest(); }

            ServiceProvider = await _userRepo.GetEntityAsync(providerId);
            Services = await _serviceRepo.GetProviderServicesAsync(providerId);

            return Page();
        }
    }
}
