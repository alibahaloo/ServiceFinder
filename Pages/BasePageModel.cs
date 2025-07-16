using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vereyon.Web;

namespace ServiceFinder.Pages
{
    public class BasePageModel : PageModel
    {
        public readonly UserManager<Data.ApplicationUser> _userManager;
        public readonly IFlashMessage _flashMessage;

        public BasePageModel(UserManager<Data.ApplicationUser> userManager, IFlashMessage flashMessage)
        {
            _userManager = userManager;
            _flashMessage = flashMessage;
        }
    }
}
