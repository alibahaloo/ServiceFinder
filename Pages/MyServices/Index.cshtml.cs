using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.MyServices
{
    [Authorize(Policy = "IsVerifiedPolicy")]
    public class IndexModel : BasePageModel
    {
        private readonly ServiceRepo _serviceRepo;
        public int CurPage { get; set; }
        public int PageCount { get; set; }
        public IndexModel(ServiceRepo serviceRepo, UserManager<ApplicationUser> userManager, IFlashMessage flashMessage)
            : base(userManager, flashMessage)
        {
            _serviceRepo = serviceRepo;
        }

        public IList<Service> Services { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int curPage = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            CurPage = curPage;

            if (user != null)
            {
                /*
                var result = await _serviceRepo.GetAllAsync(CurPage, user.Id);
                Services = result.Data;
                */

                Services = await _serviceRepo.GetProviderServicesAsync(user.Id);

                //PageCount = result.PageCount;

            }

            return Page();
        }
    }
}
