using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Admin.Users
{
    public class EditModel : AdminPageModel
    {
        private readonly IFlashMessage _flashMessage;
        private readonly UserRepo _userService;

        [BindProperty]
        public Data.InputModels.ApplicationUser ApplicationUser { get; set; } = default!;
        public EditModel(IFlashMessage flashMessage, UserRepo userService)
        {
            _flashMessage = flashMessage;
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

            ApplicationUser = new()
            {
                Id = applicationUser.Id,
                FullName = applicationUser.FullName,
                AddressLine1 = applicationUser.AddressLine1,
                AddressLine2 = applicationUser.AddressLine2,
                PostalCode = applicationUser.PostalCode,
                CityId = applicationUser.CityId,
                SubscriptionId = applicationUser.SubscriptionId,
                UserType = applicationUser.UserType,
                IsVerified = applicationUser.IsVerified,
                ServiceProviderType = applicationUser.ServiceProviderType,
                PhoneNumber = applicationUser.PhoneNumber ?? "5555555555",
                Email = applicationUser.Email ?? "email@email.com"
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Data.ApplicationUser applicationUser = new()
            {
                Id = ApplicationUser.Id,
                FullName = ApplicationUser.FullName,
                AddressLine1 = ApplicationUser.AddressLine1,
                AddressLine2 = ApplicationUser.AddressLine2,
                PostalCode = ApplicationUser.PostalCode,
                CityId = ApplicationUser.CityId,
                SubscriptionId = ApplicationUser.SubscriptionId,
                UserType = ApplicationUser.UserType,
                IsVerified = ApplicationUser.IsVerified,
                ServiceProviderType = ApplicationUser.ServiceProviderType,
                PhoneNumber = ApplicationUser.PhoneNumber,
                Email = ApplicationUser.Email
            };

            await _userService.UpdateEntityAsync(applicationUser);

            _flashMessage.Confirmation("User Updated Successfully!");

            //return RedirectToPage("./Index");
            return RedirectToPage(new {applicationUser.Id});
        }
    }
}
