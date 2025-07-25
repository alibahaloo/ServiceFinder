using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Admin.ServiceCategories
{
    public class DeleteModel : AdminPageModel
    {
        private readonly ServiceCategoryRepo _repo;
        private readonly IFlashMessage _flashMessage;

        public DeleteModel(IFlashMessage flashMessage, ServiceCategoryRepo repo)
        {
            _flashMessage = flashMessage;
            _repo = repo;
        }

        [BindProperty]
        public Category ServiceCategory { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicecategory = await _repo.GetEntityAsync(id);
            if (servicecategory == null)
            {
                return NotFound();
            }
            else
            {
                ServiceCategory = servicecategory;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            await _repo.DeleteEntityAsync(id);

            _flashMessage.Confirmation("Item Deleted Successfully!");

            return RedirectToPage("./Index");
        }
    }
}
