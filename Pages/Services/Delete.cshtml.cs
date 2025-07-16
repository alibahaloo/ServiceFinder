using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;
namespace ServiceFinder.Pages.Services
{
    [Authorize(Policy = "IsVerifiedPolicy")]
    public class DeleteModel : BasePageModel
    {
        private readonly ServiceRepo _serviceRepo;
        public DeleteModel(IFlashMessage flashMessage, ServiceRepo serviceRepo, UserManager<ApplicationUser> userManager)
            : base(userManager, flashMessage)
        {
            _serviceRepo = serviceRepo;
        }

        [BindProperty]
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

            //Check ownership
            var user = await _userManager.GetUserAsync(User);
            //Admins and providers can proceed
            if (user == null) { return Unauthorized(); }
            if (user.UserType == UserAccountRoles.ServiceProvider && !await _serviceRepo.IsUserOwnerAsync(user.Id, service.Id)) { return Unauthorized(); }

            Service = service;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
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

            //Check ownership
            var user = await _userManager.GetUserAsync(User);
            //Admins and providers can proceed
            if (user == null) { return Unauthorized(); }
            if (user.UserType == UserAccountRoles.ServiceProvider && !await _serviceRepo.IsUserOwnerAsync(user.Id, service.Id)) { return Unauthorized(); }

            if (service != null)
            {
                Service = service;
                await _serviceRepo.DeleteEntityAsync(id);
                _flashMessage.Confirmation("Service Removed Successfully!");
            }

            return RedirectToPage("./Index");
        }
    }
}
