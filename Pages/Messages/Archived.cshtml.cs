using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Utility;
using Vereyon.Web;

namespace ServiceFinder.Pages.Messages
{
    [Authorize]
    public class ArchivedModel : BasePageModel
    {
        private readonly ApplicationDbContext _context;

        public int CurPage { get; set; }
        public int PageCount { get; set; }
        public ArchivedModel(UserManager<ApplicationUser> userManager, IFlashMessage flashMessage, ApplicationDbContext context)
        : base(userManager, flashMessage)
        {
            _context = context;
        }
        public IList<MessageThread> MessageThreads { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int curPage = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Unauthorized(); }

            PagedResult<MessageThread> result;
            List<int>? supportTicketIdsAddedByUser = new();
            List<int>? bookingIdsAddedByUser = new();
            List<int>? bookingIds = new();
            List<int>? serviceIds = new();
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            //TODO ADD ALL TYPES
            switch (user.UserType)
            {
                case UserAccountRoles.ServiceProvider:
                    //Find Support Tickets
                    supportTicketIdsAddedByUser = await _context.SupportTickets
                        .Where(b => b.UserId == user.Id && b.Status == SupportTicketStatus.Resolved)
                        .Select(b => b.Id)
                        .ToListAsync();

                    //Finding all bookings that belong services that this user owns
                    bookingIds = await _context.Bookings
                        .Where(b => (b.Service.ServiceProviderId == user.Id && (b.Status == BookingStatus.Decline || b.Status == BookingStatus.Confirm)) || b.StartDate <= today)
                        .Select(b => b.Id)
                        .ToListAsync();

                    //Finding all services that this user owns
                    serviceIds = await _context.Services
                        .Where(s => s.ServiceProviderId == user.Id)
                        .Select(s => s.Id)
                        .ToListAsync();

                    result = await _context.MessageThreads
                                .Where(mt => (mt.AddedById == user.Id && mt.ResourceType == MessageResourceType.Service && mt.ArchivedOn != null) ||
                                (bookingIdsAddedByUser.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.Booking) ||
                                (supportTicketIdsAddedByUser.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.SupportTicket))
                                .GetPaged(curPage);
                    break;
                default:
                    //Find Support Tickets
                    supportTicketIdsAddedByUser = await _context.SupportTickets
                        .Where(b => b.UserId == user.Id && b.Status == SupportTicketStatus.Resolved)
                        .Select(b => b.Id)
                        .ToListAsync();

                    //Find the bookings added by user 
                    //Only the ones that are either OPEN or CONFIRMED - DECLINED and EXPIRED should go to ARCHIVE
                    bookingIdsAddedByUser = await _context.Bookings
                        .Where(b => (b.AddedBy.Id == user.Id && (b.Status == BookingStatus.Decline || b.Status == BookingStatus.Confirm)) || b.StartDate <= today)
                        .Select(b => b.Id)
                        .ToListAsync();

                    result = await _context.MessageThreads
                               .Where(mt => (mt.AddedById == user.Id && mt.ResourceType == MessageResourceType.Service && mt.ArchivedOn != null) ||
                               (bookingIdsAddedByUser.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.Booking) ||
                               (supportTicketIdsAddedByUser.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.SupportTicket))
                               .GetPaged(curPage);
                    break;
            }


            MessageThreads = result.Data;
            PageCount = result.PageCount;

            return Page();
        }
    }
}
