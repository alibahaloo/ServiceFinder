using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using Vereyon.Web;
namespace ServiceFinder.Pages.Admin.Provinces
{
    public class DeleteModel : AdminPageModel
    {
        private readonly ServiceFinder.Data.ApplicationDbContext _context;
        private readonly IFlashMessage _flashMessage;

        public DeleteModel(ServiceFinder.Data.ApplicationDbContext context, IFlashMessage flashMessage)
        {
            _context = context;
            _flashMessage = flashMessage;
        }

        [BindProperty]
        public Province Province { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Provinces == null)
            {
                return NotFound();
            }

            var province = await _context.Provinces.FirstOrDefaultAsync(m => m.Id == id);

            if (province == null)
            {
                return NotFound();
            }
            else
            {
                Province = province;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.Provinces == null)
            {
                return NotFound();
            }
            var province = await _context.Provinces.FindAsync(id);

            if (province != null)
            {
                Province = province;
                _context.Provinces.Remove(Province);
                await _context.SaveChangesAsync();

                _flashMessage.Confirmation("Item Deleted Successfully!");

            }

            return RedirectToPage("./Index");
        }
    }
}
