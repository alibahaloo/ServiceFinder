using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;

namespace ServiceFinder.Pages.Admin.Cities
{
    public class DetailsModel : AdminPageModel
    {
        private readonly CityRepo _cityService;

        public DetailsModel(CityRepo cityService)
        {
            this._cityService = cityService;
        }

        public City City { get; set; } = default!;

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
                City = city;
            }
            return Page();
        }
    }
}
