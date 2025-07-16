using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Admin.Cities
{
    public class CreateModel : AdminPageModel
    {
        private readonly CityRepo _cityService;
        private readonly IFlashMessage _flashMessage;

        public CreateModel(IFlashMessage flashMessage, CityRepo cityService)
        {
            _flashMessage = flashMessage;
            _cityService = cityService;
        }

        public IActionResult OnGet()
        {
            return Page();
        }
        [BindProperty]
        public Data.InputModels.City City { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (City.IsNavItem)
            {
                if (!_cityService.ValidateForSavingNavItem())
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
                Name = City.Name,
                ProvinceId = City.ProvinceId,
                IsNavItem = City.IsNavItem,
            };

            await _cityService.CreateEntityAsync(city);

            _flashMessage.Confirmation("Item Created Successfully!");

            return RedirectToPage("./Index");
        }
    }
}
