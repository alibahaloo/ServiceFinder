using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;
namespace ServiceFinder.Pages.Admin.Industries
{
    public class EditModel : AdminPageModel
    {
        private readonly IndustryRepo _repo;
        private readonly IFlashMessage _flashMessage;

        public EditModel(IFlashMessage flashMessage, IndustryRepo repo)
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

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _repo.UpdateEntityAsync(Industry);
            _flashMessage.Confirmation("Item Updated Successfully!");

            return RedirectToPage("./Index");
        }
    }
}
