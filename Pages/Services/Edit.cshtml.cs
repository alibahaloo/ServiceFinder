using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;
namespace ServiceFinder.Pages.Services
{
    [Authorize(Policy = "IsVerifiedPolicy")]
    public class EditModel : BasePageModel
    {
        private readonly ServiceRepo _serviceRepo;

        [BindProperty]
        public List<int> SelectedCitiesIds { get; set; } = new List<int>();
        [BindProperty]
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
        [BindProperty]
        public Data.InputModels.Service Input { get; set; } = default!;
        public EditModel(IFlashMessage flashMessage, ServiceRepo serviceRepo, UserManager<Data.ApplicationUser> userManager)
            : base(userManager, flashMessage)
        {
            _serviceRepo = serviceRepo;
        }

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

            Input = new()
            {
                Id = service.Id,
                LogoUrl = service.LogoUrl,
                Name = service.Name,
                Description = service.Description,
                YearsInBusiness = service.YearsInBusiness,
                ServiceProviderId = service.ServiceProviderId,
                AddressLine1 = service.AddressLine1,
                AddressLine2 = service.AddressLine2,
                PostalCode = service.PostalCode,
                CityId = service.CityId,
                PhoneNumber = service.PhoneNumber,
                Email = service.Email,
                Website = service.Website,
                HourlyRate = service.HourlyRate,
                ServiceCategories = service.ServiceCategories,
                ServiceCities = service.ServiceCities,
            };

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
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

            if (!ModelState.IsValid || Input == null)
            {
                return Page();
            }

            var service = await _serviceRepo.GetEntityAsync(Input.Id);
            if (service == null)
            {
                return NotFound();
            }

            //Check ownership
            var user = await _userManager.GetUserAsync(User);
            //Admins and providers can proceed
            if (user == null) { return Unauthorized(); }
            if (user.UserType == UserAccountRoles.ServiceProvider && !await _serviceRepo.IsUserOwnerAsync(user.Id, service.Id)) { return Unauthorized(); }

            service.Name = Input.Name;
            service.LogoUrl = Input.LogoUrl;
            service.Description = Input.Description;
            service.YearsInBusiness = Input.YearsInBusiness;
            service.Website = Input.Website;
            service.AddressLine1 = Input.AddressLine1;
            service.AddressLine2 = Input.AddressLine2;
            service.PostalCode = Input.PostalCode;
            service.CityId = Input.CityId;
            service.PhoneNumber = Input.PhoneNumber;
            service.Email = Input.Email;
            service.HourlyRate = Input.HourlyRate;

            await _serviceRepo.UpdateEntityAsync(service, SelectedCitiesIds, SelectedCategoryIds);
            _flashMessage.Confirmation("Service Updated Successfully!");

            return Redirect($"/Services/Details?Id={service.Id}");
        }
    }
}
