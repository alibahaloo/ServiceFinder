using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using Vereyon.Web;

namespace ServiceFinder.Pages.Reviews
{
    [Authorize]
    public class DeleteModel : BasePageModel
    {
        private readonly Repos.ReviewRepo _reviewRepo;
        public DeleteModel(Repos.ReviewRepo reviewRepo, IFlashMessage flashMessage, UserManager<Data.ApplicationUser> userManager)
            : base(userManager, flashMessage)
        {
            _reviewRepo = reviewRepo;
        }

        [BindProperty]
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

            //Check ownership
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !await _reviewRepo.IsUserOwner(user.Id, review.Id)) { return Unauthorized(); }

            Review = review;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            var review = await _reviewRepo.GetEntityAsync(Review.Id);
            if (review == null)
            {
                return NotFound();
            }

            //Check ownership
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !await _reviewRepo.IsUserOwner(user.Id, review.Id)) { return Unauthorized(); }

            Review = review;
            await _reviewRepo.DeleteEntityAsync(id);
            return RedirectToPage("/Reviews/MyReviews");
        }
    }
}
