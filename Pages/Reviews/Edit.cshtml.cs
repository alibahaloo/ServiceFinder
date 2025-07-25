using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace ServiceFinder.Pages.Reviews
{
    [Authorize]
    public class EditModel : BasePageModel
    {
        private readonly Repos.ReviewRepo _reviewRepo;
        public EditModel(Repos.ReviewRepo reviewRepo, IFlashMessage flashMessage, UserManager<Data.ApplicationUser> userManager)
            : base(userManager, flashMessage)
        {
            _reviewRepo = reviewRepo;
        }

        public async Task<IActionResult> OnGet(int? id)
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

            Input = new Data.InputModels.Review
            {
                Id = review.Id,
                Body = review.Body,
                Title = review.Title,
                Stars = review.Stars,
                ServiceId = review.ServiceId,
            };

            return Page();
        }

        [BindProperty]
        public Data.InputModels.Review Input { get; set; } = default!;


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var review = await _reviewRepo.GetEntityAsync(Input.Id);
            if (review == null)
            {
                return NotFound();
            }

            //Check ownership
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !await _reviewRepo.IsUserOwner(user.Id, review.Id)) { return Unauthorized(); }

            review.Body = Input.Body;
            review.Title = Input.Title;
            review.Stars = Input.Stars;

            await _reviewRepo.UpdateEntityAsync(review);
            _flashMessage.Confirmation("Item Updated Successfully!");

            return RedirectToPage("./MyReviews");
        }
    }
}
