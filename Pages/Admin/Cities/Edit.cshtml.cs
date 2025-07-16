using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;
namespace ServiceFinder.Pages.Admin.Cities
{
    public class EditModel : AdminPageModel
    {
        private readonly CityRepo _cityService;
        private readonly IFlashMessage _flashMessage;

        public EditModel(IFlashMessage flashMessage, CityRepo cityService)
        {
            _flashMessage = flashMessage;
            _cityService = cityService;
        }

        [BindProperty]
        public Data.InputModels.City City { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _cityService.GetEntityAsync(id);
            if (city == null)
            {
                return NotFound();
            }
            else
            {
                City = new Data.InputModels.City
                {
                    Id = city.Id,
                    Name = city.Name,
                    ProvinceId = city.ProvinceId,
                    IsNavItem = city.IsNavItem,
                };
            }
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (City.IsNavItem)
            {
                if (!_cityService.ValidateForSavingNavItem(City.Id))
                {
                    _flashMessage.Danger("You can't have more than 5 items in the navigation bar. Go back and remove one to add");
                    return Page();
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Data.City city = new()
            {
                Id = City.Id,
                Name = City.Name,
                ProvinceId = City.ProvinceId,
                IsNavItem = City.IsNavItem,
            };

            await _cityService.UpdateEntityAsync(city);

            _flashMessage.Confirmation("Item Updated Successfully!");

            return RedirectToPage("./Index");
        }
    }
}
