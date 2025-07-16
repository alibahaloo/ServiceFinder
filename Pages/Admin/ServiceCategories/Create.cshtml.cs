using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Admin.ServiceCategories
{
    public class CreateModel : AdminPageModel
    {
        private readonly ServiceCategoryRepo _repo;
        private readonly IndustryRepo _industryRepo;
        private readonly IFlashMessage _flashMessage;
        public CreateModel(IFlashMessage flashMessage, ServiceCategoryRepo repo, IndustryRepo industryRepo)
        {
            _flashMessage = flashMessage;
            _repo = repo;
            _industryRepo = industryRepo;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Data.InputModels.Category ServiceCategory { get; set; } = default!;


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (ServiceCategory.IsNavItem)
            {
                if (!_repo.ValidateForSavingNavItem())
                {
                    _flashMessage.Danger("You can't have more than 5 items in the navigation bar. Go back and remove one to add");
                    return Page();
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Data.Category entity = new()
            {
                Name = ServiceCategory.Name,
                IndustryId = ServiceCategory.IndustryId,
                IsNavItem = ServiceCategory.IsNavItem,
            };

            await _repo.CreateEntityAsync(entity);

            _flashMessage.Confirmation("Item Created Successfully!");
            return RedirectToPage("./Index");
        }
    }
}
