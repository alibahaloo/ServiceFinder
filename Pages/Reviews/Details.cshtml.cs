using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using Vereyon.Web;

namespace ServiceFinder.Pages.Reviews
{
    [Authorize]
    public class DetailsModel : BasePageModel
    {
        private readonly Repos.ReviewRepo _reviewRepo;

        public DetailsModel(Repos.ReviewRepo reviewRepo, IFlashMessage flashMessage, UserManager<Data.ApplicationUser> userManager)
            : base(userManager, flashMessage)
        {
            _reviewRepo = reviewRepo;
        }

        public Review Review { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _reviewRepo.GetEntityAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            Review = review;
            return Page();
        }
    }
}
