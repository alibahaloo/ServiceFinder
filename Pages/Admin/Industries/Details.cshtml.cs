using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;

namespace ServiceFinder.Pages.Admin.Industries
{
    public class DetailsModel : AdminPageModel
    {
        private readonly IndustryRepo _repo;

        public DetailsModel(IndustryRepo repo)
        {
            _repo = repo;
        }

        public Industry Industry { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var industry = await _repo.GetEntityAsync(id);
            if (industry == null)
            {
                return NotFound();
            }
            Industry = industry;
            return Page();
        }
    }
}
