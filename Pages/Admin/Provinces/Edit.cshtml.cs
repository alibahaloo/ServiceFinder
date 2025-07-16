using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using Vereyon.Web;

namespace ServiceFinder.Pages.Admin.Provinces
{
    public class EditModel : AdminPageModel
    {
        private readonly ServiceFinder.Data.ApplicationDbContext _context;
        private readonly IFlashMessage _flashMessage;

        public EditModel(ServiceFinder.Data.ApplicationDbContext context, IFlashMessage flashMessage)
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
            Province = province;
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

            _context.Attach(Province).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _flashMessage.Confirmation("Item Updated Successfully!");

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProvinceExists(Province.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ProvinceExists(int id)
        {
            return (_context.Provinces?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
