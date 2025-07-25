using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;

namespace ServiceFinder.Pages.Admin.Users
{
    public class DetailsModel : AdminPageModel
    {
        private readonly UserRepo _userService;
        [BindProperty]
        public ApplicationUser ApplicationUser { get; set; } = default!;
        public DetailsModel(UserRepo userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _userService.GetEntityAsync(id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            ApplicationUser = applicationUser;

            return Page();
        }
    }
}
