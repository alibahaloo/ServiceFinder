using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;
namespace ServiceFinder.Pages.Services
{
    [Authorize(Policy = "IsVerifiedPolicy")]
    public class CreateModel : BasePageModel
    {
        private readonly ServiceRepo _serviceRepo;

        [BindProperty]
        public Data.InputModels.Service Service { get; set; } = default!;
        [BindProperty]
        public List<int> SelectedCitiesIds { get; set; } = new List<int>();
        [BindProperty]
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
        public CreateModel(IFlashMessage flashMessage, ServiceRepo serviceRepo, UserManager<ApplicationUser> userManager)
            : base(userManager, flashMessage)
        {
            _serviceRepo = serviceRepo;
        }

        public async Task<IActionResult> OnGet(string? providerId)
        {
            string actualProviderId;

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (providerId != null)
            {
                //check if the provider is an actual user
                var provider = await _userManager.FindByIdAsync(providerId);

                if (provider == null) { return BadRequest(); }

                if (provider.UserType != UserAccountRoles.ServiceProvider) { return BadRequest(); }

                //also, check that admins can only do this
                if (user.UserType != UserAccountRoles.Admin)
                {
                    return Unauthorized();
                }

                actualProviderId = providerId;
            }
            else
            {
                actualProviderId = user.Id;
            }

            Service = new Data.InputModels.Service { ServiceProviderId = actualProviderId };

            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (SelectedCitiesIds.Count == 0)
            {
                _flashMessage.Danger("You need to at least select 1 city!");
                return Page();
            }

            if (SelectedCategoryIds.Count == 0)
            {
                _flashMessage.Danger("You need to at least select 1 category!");
                return Page();
            }

            if (!ModelState.IsValid || Service == null)
            {
                return Page();
            }

            Service service = new()
            {
                Name = Service.Name,
                LogoUrl = Service.LogoUrl,
                Description = Service.Description,
                YearsInBusiness = Service.YearsInBusiness,
                ServiceProviderId = Service.ServiceProviderId,
                AddressLine1 = Service.AddressLine1,
                AddressLine2 = Service.AddressLine2,
                PostalCode = Service.PostalCode,
                CityId = Service.CityId,
                PhoneNumber = Service.PhoneNumber,
                Email = Service.Email,
                Website = Service.Website,
                HourlyRate = Service.HourlyRate,
            };

            await _serviceRepo.CreateEntityAsync(service, SelectedCitiesIds, SelectedCategoryIds);
            _flashMessage.Confirmation("Service Added Successfully!");


            if (user.UserType == UserAccountRoles.Admin)
            {
                return Redirect($"/Admin/Services/Index?providerId={Service.ServiceProviderId}");
            }
            else
            {
                return Redirect($"/MyServices/Index");
            }
        }
    }
}
