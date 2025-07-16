using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Admin.Users
{
    public class DeleteModel : AdminPageModel
    {
        private readonly IFlashMessage _flashMessage;
        private readonly UserRepo _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        public DeleteModel(IFlashMessage flashMessage, UserRepo userService, UserManager<ApplicationUser> userManager)
        {
            _flashMessage = flashMessage;
            _userService = userService;
            _userManager = userManager;
        }

        [BindProperty]
        public ApplicationUser ApplicationUser { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // admin cannot delete themselves
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (id == user.Id)
            {
                _flashMessage.Danger("You cannot delete your own account!");
                return RedirectToPage("./Index");
            }

            var applicationUser = await _userService.GetEntityAsync(id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            ApplicationUser = applicationUser;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //admin cannot delete themselves
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (id == user.Id)
            {
                _flashMessage.Danger("You cannot delete your own account!");
                return RedirectToPage("./Index");
            }

            await _userService.DeleteEntityAsync(id);

            _flashMessage.Confirmation("Item Deleted Successfully!");

            return RedirectToPage("./Index");
        }
    }
}
