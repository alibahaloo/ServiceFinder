using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Reviews
{
    [Authorize]
    public class MyReviewsModel : BasePageModel
    {
        private readonly ReviewRepo _reviewRepo;
        public int CurPage { get; set; }
        public int PageCount { get; set; }
        public IList<Review> Reviews { get; set; } = default!;
        public MyReviewsModel(UserManager<ApplicationUser> userManager, ReviewRepo reviewRepo, IFlashMessage flashMessage)
            : base(userManager, flashMessage)
        {
            _reviewRepo = reviewRepo;
        }

        public async Task<IActionResult> OnGet(int curPage = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            CurPage = curPage;
            if (user == null)
            {
                return Unauthorized();
            }

            Utility.PagedResult<Review>? result;

            if (user.UserType == UserAccountRoles.ServiceProvider)
            {
                result = await _reviewRepo.GetAllByProviderAsync(CurPage, user.Id);
            }
            else
            {
                result = await _reviewRepo.GetAllBySenderAsync(CurPage, user.Id);
            }


            Reviews = result.Data;
            PageCount = result.PageCount;

            return Page();
        }
    }
}
