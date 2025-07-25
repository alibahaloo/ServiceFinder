using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Utility;
using System.ComponentModel.DataAnnotations;
using Vereyon.Web;

namespace ServiceFinder.Pages.Messages
{
    [Authorize]
    public class InboxModel : BasePageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly Repos.ServiceRepo _serviceRepo;
        public class InputModel
        {
            public string Search { get; set; } = string.Empty;
            [Display(Name = "Type")]
            public MessageResourceType MessageResourceType { get; set; }
            [Display(Name = "Service")]
            public int ServiceId { get; set; }
        }
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public Service Service { get; set; } = default!;

        public int CurPage { get; set; }
        public int PageCount { get; set; }
        public InboxModel(UserManager<ApplicationUser> userManager, IFlashMessage flashMessage, ApplicationDbContext context, Repos.ServiceRepo serviceRepo)
        : base(userManager, flashMessage)
        {
            _context = context;
            _serviceRepo = serviceRepo;
        }

        public IList<MessageThread> MessageThreads { get; set; } = default!;

        public async Task<IActionResult> OnPostArchiveAsync(int messageThreadId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Unauthorized(); }

            var messageThread = await _context.MessageThreads.FirstOrDefaultAsync(mt => mt.Id == messageThreadId);

            if (messageThread == null) { return NotFound(); }

            messageThread.ArchivedOn = DateTime.UtcNow;
            messageThread.ArchiverId = user.Id;
            await _context.SaveChangesAsync();

            _flashMessage.Warning("Message Archived. You can find this thread in your `Archived` list");
            return Redirect("/Messages/Inbox");
        }

        public async Task<IActionResult> OnGetAsync(InputModel input, int curPage = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Unauthorized(); }

            PagedResult<MessageThread> result = new();
            List<int>? supportTicketIdsAddedByUser = [];
            List<int>? bookingIdsAddedByUser = [];
            List<int>? bookingIds = [];
            List<int>? serviceIds = [];
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            Input = input;

            if (input.ServiceId != 0)
            {
                //User has selected a particular service to search for
                serviceIds.Add(input.ServiceId);
            }
            else
            {
                if (user.UserType == UserAccountRoles.ServiceProvider)
                {
                    //Finding all services that this user owns
                    serviceIds = await _context.Services
                        .Where(s => s.ServiceProviderId == user.Id)
                        .Select(s => s.Id)
                        .ToListAsync();
                }
            }

            //Find Support Tickets regardless of the user type
            supportTicketIdsAddedByUser = await _context.SupportTickets
                .Where(b => b.UserId == user.Id && b.Status == SupportTicketStatus.Open)
                .Select(b => b.Id)
                .ToListAsync();

            if (user.UserType == UserAccountRoles.ServiceProvider)
            {
                //Finding all bookings that belong services that this user owns
                bookingIds = await _context.Bookings
                    .Where(b => b.Service.ServiceProviderId == user.Id && b.Status == BookingStatus.Open && b.StartDate > today)
                    .Select(b => b.Id)
                    .ToListAsync();

                switch (input.MessageResourceType)
                {
                    case MessageResourceType.All:

                        result = await _context.MessageThreads
                            .Include(mt => mt.AddedBy)
                            .Where(mt =>
                                (
                                    (bookingIds.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.Booking)
                                    || (serviceIds.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.Service)
                                    || (supportTicketIdsAddedByUser.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.SupportTicket)
                                )
                                && mt.ArchivedOn == null)
                            .GetPaged(curPage);
                        break;
                    case MessageResourceType.Service:
                        result = await _context.MessageThreads
                            .Include(mt => mt.AddedBy)
                            .Where(mt =>
                                (
                                   serviceIds.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.Service
                                )
                                && mt.ArchivedOn == null)
                            .GetPaged(curPage);
                        break;
                    case MessageResourceType.Booking:
                        result = await _context.MessageThreads
                            .Include(mt => mt.AddedBy)
                            .Where(mt =>
                                (
                                   bookingIds.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.Booking
                                )
                                && mt.ArchivedOn == null)
                            .GetPaged(curPage);
                        break;
                    case MessageResourceType.SupportTicket:
                        result = await _context.MessageThreads
                            .Include(mt => mt.AddedBy)
                            .Where(mt =>
                                (
                                    supportTicketIdsAddedByUser.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.SupportTicket
                                )
                                && mt.ArchivedOn == null)
                            .GetPaged(curPage);
                        break;
                }
            }
            else
            {
                //Find the bookings added by user 
                //Only the ones that are either OPEN or CONFIRMED AND are not expired - DECLINED and EXPIRED should go to ARCHIVE
                bookingIdsAddedByUser = await _context.Bookings
                    .Where(b => b.AddedBy.Id == user.Id && b.Status == BookingStatus.Open && b.StartDate > today)
                    .Select(b => b.Id)
                    .ToListAsync();

                switch (input.MessageResourceType)
                {
                    case MessageResourceType.All:
                        result = await _context.MessageThreads
                            .Where(mt =>
                                    (
                                        (mt.AddedById == user.Id && mt.ResourceType == MessageResourceType.Service) ||
                                        (bookingIdsAddedByUser.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.Booking) ||
                                        (supportTicketIdsAddedByUser.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.SupportTicket)
                                    )
                                    && mt.ArchivedOn == null)
                            .GetPaged(curPage);
                        break;
                    case MessageResourceType.Service:
                        result = await _context.MessageThreads
                            .Where(mt => mt.AddedById == user.Id && mt.ResourceType == MessageResourceType.Service && mt.ArchivedOn == null)
                            .GetPaged(curPage);
                        break;
                    case MessageResourceType.Booking:
                        result = await _context.MessageThreads
                            .Where(mt => bookingIdsAddedByUser.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.Booking && mt.ArchivedOn == null)
                            .GetPaged(curPage);
                        break;
                    case MessageResourceType.SupportTicket:
                        result = await _context.MessageThreads
                            .Where(mt => supportTicketIdsAddedByUser.Contains(mt.ResourceId) && mt.ResourceType == MessageResourceType.SupportTicket && mt.ArchivedOn == null)
                            .GetPaged(curPage);
                        break;
                }
            }
            
            IList<Service> serviceList;
            if (user.UserType == UserAccountRoles.ServiceProvider)
            {
                serviceList = await _serviceRepo.GetProviderServicesAsync(user.Id);
            }
            else
            {
                serviceList = await _serviceRepo.GetAllAsync();
            }

            SelectListItem firstItem = new() { Value = "0", Text = "All Services" };
            List<SelectListItem> serviceSelectList = new SelectList(serviceList, "Id", "Name").ToList();
            serviceSelectList.Insert(0, firstItem);
            ViewData["ServiceId"] = serviceSelectList;

            //Find all new Threads for the user
            var newThreads = await _context.NewMessageThreadAlerts.Where(mt => mt.ApplicationUserId ==  user.Id).ToListAsync();
            var newThreadIds = newThreads.Select(nt => nt.MessageThreadId).ToHashSet();
            
            MessageThreads = result.Data;

            //Iterate over the MessageThreads and mark IsNew if the ID exists in newThreadIds
            foreach (var thread in MessageThreads)
            {
                thread.IsNew = newThreadIds.Contains(thread.Id);
            }

            PageCount = result.PageCount;

            return Page();
        }
    }
}
