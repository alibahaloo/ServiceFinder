using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using Vereyon.Web;
namespace ServiceFinder.Pages.Admin.Provinces
{
    public class CreateModel : AdminPageModel
    {
        private readonly ServiceFinder.Data.ApplicationDbContext _context;
        private readonly IFlashMessage _flashMessage;
        public CreateModel(ServiceFinder.Data.ApplicationDbContext context, IFlashMessage flashMessage)
        {
            _context = context;
            _flashMessage = flashMessage;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Province Province { get; set; } = default!;


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || _context.Provinces == null || Province == null)
            {
                return Page();
            }

            _context.Provinces.Add(Province);
            await _context.SaveChangesAsync();
            _flashMessage.Confirmation("Item Created Successfully!");

            return RedirectToPage("./Index");
        }
    }
}
