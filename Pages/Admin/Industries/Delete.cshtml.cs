using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;
namespace ServiceFinder.Pages.Admin.Industries
{
    public class DeleteModel : AdminPageModel
    {
        private readonly IFlashMessage _flashMessage;
        private readonly IndustryRepo _repo;

        public DeleteModel(IFlashMessage flashMessage, IndustryRepo repo)
        {
            _flashMessage = flashMessage;
            _repo = repo;
        }

        [BindProperty]
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
