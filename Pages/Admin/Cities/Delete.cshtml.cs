using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;
namespace ServiceFinder.Pages.Admin.Cities
{
    public class DeleteModel : AdminPageModel
    {
        private readonly CityRepo _cityService;
        private readonly IFlashMessage _flashMessage;
        public DeleteModel(IFlashMessage flashMessage, CityRepo cityService)
        {
            _flashMessage = flashMessage;
            this._cityService = cityService;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            await _cityService.DeleteEntityAsync(id);

            _flashMessage.Confirmation("Item Deleted Successfully!");
            return RedirectToPage("./Index");
        }
    }
}
