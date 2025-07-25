using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;
namespace ServiceFinder.Pages.Admin.Industries
{
    public class CreateModel : AdminPageModel
    {
        private readonly IndustryRepo _repo;
        private readonly IFlashMessage _flashMessage;
        public CreateModel(IFlashMessage flashMessage, IndustryRepo repo)
        {
            _flashMessage = flashMessage;
            _repo = repo;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Industry Industry { get; set; } = default!;


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _repo.CreateEntityAsync(Industry);
            _flashMessage.Confirmation("Item Created Successfully!");
            return RedirectToPage("./Index");
        }
    }
}
