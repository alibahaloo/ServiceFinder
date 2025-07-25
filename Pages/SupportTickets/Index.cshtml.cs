using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.SupportTickets
{
    public class IndexModel : BasePageModel
    {
        private readonly SupportTicketRepo _supportTicketRepo;
        public class InputModel
        {
            public SupportTicketStatus Status { get; set; }
        }
        public IndexModel(UserManager<Data.ApplicationUser> userManager, SupportTicketRepo supportTicketRepo, IFlashMessage flashMessage)
            : base(userManager, flashMessage)
        {
            _supportTicketRepo = supportTicketRepo;
        }
        public IList<Data.SupportTicket> SupportTickets { get; set; } = default!;
        public int CurPage { get; set; }
        public int PageCount { get; set; }
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public async Task<IActionResult> OnGet(InputModel input, int curPage = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Email == null) { return Unauthorized(); }

            Input = input;

            var result = await _supportTicketRepo.GetMySupportTicketsAsync(curPage, user.Id, input.Status);

            CurPage = curPage;
            SupportTickets = result.Data;
            PageCount = result.PageCount;
            return Page();
        }
    }
}
