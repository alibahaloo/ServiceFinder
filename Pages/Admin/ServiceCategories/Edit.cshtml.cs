using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Admin.ServiceCategories
{
    public class EditModel : AdminPageModel
    {
        private readonly ServiceCategoryRepo _repo;
        private readonly IFlashMessage _flashMessage;

        public EditModel(IFlashMessage flashMessage, ServiceCategoryRepo repo)
        {
            _flashMessage = flashMessage;
            _repo = repo;
        }

        [BindProperty]
        public Data.InputModels.Category Category { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _repo.GetEntityAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            else
            {
                Category = new()
                {
                    Id = category.Id,
                    Name = category.Name,
                    IndustryId = category.IndustryId,
                    IsNavItem = category.IsNavItem,
                };
            }
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (Category.IsNavItem)
            {
                if (!_repo.ValidateForSavingNavItem(Category.Id))
                {
                    _flashMessage.Danger("You can't have more than 5 items in the navigation bar. Go back and remove one to add");
                    return Page();
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Data.Category category = new()
            {
                Id = Category.Id,
                Name = Category.Name,
                IndustryId = Category.IndustryId,
                IsNavItem = Category.IsNavItem,
            };

            await _repo.UpdateEntityAsync(category);
            _flashMessage.Confirmation("Item Updated Successfully!");

            return RedirectToPage("./Index");
        }
    }
}
