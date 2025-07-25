using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Services
{
    public class DetailsModel : BasePageModel
    {
        private readonly ServiceRepo _serviceRepo;

        public DetailsModel(ServiceRepo serviceRepo, IFlashMessage flashMessage, UserManager<Data.ApplicationUser> userManager)
            : base(userManager, flashMessage)
        {
            _serviceRepo = serviceRepo;
        }

        public Service Service { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _serviceRepo.GetEntityAsync(id);

            if (service == null)
            {
                return NotFound();
            }
            else
            {
                Service = service;
            }
            return Page();
        }
    }
}
