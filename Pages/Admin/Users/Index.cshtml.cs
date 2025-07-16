using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using ServiceFinder.Utility;
using System.ComponentModel.DataAnnotations;
using Vereyon.Web;

namespace ServiceFinder.Pages.Admin.Users
{
    public class IndexModel : AdminPageModel
    {
        public class InputModel
        {
            public string Search { get; set; } = default!;

            [Display(Name = "User Type")]
            public string UserType { get; set; } = default!;

            [Display(Name = "Subscription")]
            public int SubscriptionId { get; set; } = default!;

            [Display(Name = "Show unverified users only")]
            public bool Verified { get; set; }
        }

        private readonly UserRepo _userRepo;
        private readonly IFlashMessage _flashMessage;
        private readonly SupportTicketRepo _supportTicketRepo;
        private readonly EmailService _emailService;
        public int CurPage { get; set; }
        public int PageCount { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public IndexModel(UserRepo userRepo, IFlashMessage flashMessage, SupportTicketRepo supportTicketRepo, EmailService emailService)
        {
            _userRepo = userRepo;
            _flashMessage = flashMessage;
            _supportTicketRepo = supportTicketRepo;
            _emailService = emailService;
        }

        public IList<Data.ApplicationUser> ApplicationUsers { get; set; } = default!;
        public async Task<IActionResult> OnPostConfirmAsync(string Id, string command)
        {
            var user = await _userRepo.GetEntityAsync(Id);

            if (user == null) { return BadRequest(); }

            switch (command)
            {
                case "confirm":
                    user.EmailConfirmed = true;
                    break;
                case "deny":
                    user.EmailConfirmed = false;
                    break;
            }

            //Update the user
            await _userRepo.UpdateEntityAsync(user);

            _flashMessage.Confirmation("Account Updated Successfully!");

            string refererUrl = Request.Headers["Referer"].ToString();
            return Redirect(refererUrl);
        }
        public async Task<IActionResult> OnPostVerifyAsync(string Id, string command)
        {

            var user = await _userRepo.GetEntityAsync(Id);

            if (user == null) { return BadRequest(); }

            switch (command)
            {
                case "verify":
                    user.IsVerified = true;
                    break;
                case "deny":
                    user.IsVerified = false;
                    break;
            }

            await _userRepo.UpdateEntityAsync(user);

            //Mark the support ticket as resolved
            if (user.Email != null) await _supportTicketRepo.ResolveSupportTicket(user.Email);

            //Send an email to the account holder and notify
            if (user.Email != null && user.UserType == UserAccountRoles.ServiceProvider)
            {
                _emailService.SendAccountVerified(user.Email);
            }

            _flashMessage.Confirmation("Account Verification Status Updated Successfully!");

            if (Request.Headers["Referer"].ToString().Contains("SupportTickets"))
            {
                return Redirect("/SupportTickets/Admin");
            }
            else
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }
        }

        public async Task OnGetAsync(InputModel input, int curPage = 1)
        {
            CurPage = curPage;
            //Doing the search
            var result = new PagedResult<ApplicationUser>();
            var query = _userRepo.GetSearchQuery();

            if (input.Search != string.Empty && input.Search != null)
                query = query.Where(s => s.FullName.ToLower().Contains(input.Search.ToLower()) ||
                (s.Email != null && s.Email.ToLower().Contains(input.Search.ToLower())));
            if (input.SubscriptionId != 0)
                query = query.Where(s => s.SubscriptionId == input.SubscriptionId);
            if (input.UserType != "All" && input.UserType is not null)
                query = query.Where(s => s.UserType == input.UserType);
            if (input.Verified != false)
                query = query.Where(s => s.IsVerified == false && s.UserType == UserAccountRoles.ServiceProvider);

            result = await _userRepo.SearchQuery(CurPage, query);

            Input = input;

            ApplicationUsers = result.Data;
            PageCount = result.PageCount;
        }
    }
}
