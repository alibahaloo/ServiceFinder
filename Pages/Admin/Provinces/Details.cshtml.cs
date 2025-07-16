using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;

namespace ServiceFinder.Pages.Admin.Provinces
{
    public class DetailsModel : AdminPageModel
    {
        private readonly ServiceFinder.Data.ApplicationDbContext _context;

        public DetailsModel(ServiceFinder.Data.ApplicationDbContext context)
        {
            _context = context;
        }

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
    }
}
