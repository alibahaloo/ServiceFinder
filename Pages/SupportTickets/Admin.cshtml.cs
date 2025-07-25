using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Pages.Admin;
using ServiceFinder.Repos;

namespace ServiceFinder.Pages.SupportTickets
{
    public class AdminModel : AdminPageModel
    {
        private readonly SupportTicketRepo _supportTicketRepo;

        public class InputModel
        {
            public string Search { get; set; } = string.Empty;
            public SupportTicketStatus Status { get; set; }
        }

        public AdminModel(Repos.SupportTicketRepo supportTicketRepo)
        {
            _supportTicketRepo = supportTicketRepo;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public IList<SupportTicket> SupportTickets { get; set; } = default!;
        public int CurPage { get; set; }
        public int PageCount { get; set; }
        public async Task<IActionResult> OnGetAsync(InputModel input, int curPage = 1)
        {
            Input = input;

            var result = await _supportTicketRepo.GetAllSupportTicketsAsync(curPage, input.Status, input.Search);

            CurPage = curPage;
            SupportTickets = result.Data;
            PageCount = result.PageCount;
            return Page();
            /*
            if (_context.SupportTickets != null)
            {
                SupportTicket = await _context.SupportTickets
                .Include(s => s.User).ToListAsync();
            }
            */
        }
    }
}
