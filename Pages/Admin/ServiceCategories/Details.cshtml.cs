using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;

namespace ServiceFinder.Pages.Admin.ServiceCategories
{
    public class DetailsModel : AdminPageModel
    {
        private readonly ServiceCategoryRepo _repo;
        public DetailsModel(ServiceCategoryRepo repo)
        {
            _repo = repo;
        }

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
    }
}
