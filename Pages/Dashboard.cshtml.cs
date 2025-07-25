using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;

namespace ServiceFinder.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        //Service Provider
        public int ServiceCount { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;
        public int MessageCount { get; set; } = 0;
        public int AllBookingCount { get; set; } = 0;
        public int OpenBookingCount { get; set; } = 0;
        public int ConfirmedBookingCount { get; set; } = 0;
        public int RejectedBookingCount { get; set; } = 0;
        public string UserType { get; set; } = default!;
        //Admin counts
        public int TotalServices { get; set; } = 0;
        public int TotalUsers { get; set; } = 0;
        public int TotalBookings { get; set; } = 0;
        public int TotalMessages { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public int TotalCategories { get; set; } = 0;
        public int TotalCities { get; set; } = 0;
        public int TotalProvinces { get; set; } = 0;
        public DashboardModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Unauthorized(); }

            UserType = user.UserType;

            if (UserType == UserAccountRoles.Admin)
            {
                TotalServices = await _context.Services.CountAsync();
                TotalUsers = await _context.ApplicationUsers.CountAsync();
                TotalBookings = await _context.Bookings.CountAsync();
                TotalMessages = await _context.MessageThreads.CountAsync();
                TotalReviews = await _context.Reviews.CountAsync();
                TotalCategories = await _context.Categories.CountAsync();
                TotalCities = await _context.Cities.CountAsync();
                TotalProvinces = await _context.Provinces.CountAsync();
            }

            if (UserType == UserAccountRoles.ServiceProvider)
            {
                //Servic Provider Stats

                //Find Support Tickets
                var supportTicketIdsAddedByUser = await _context.SupportTickets
                .Where(b => b.UserId == user.Id)
                .Select(b => b.Id)
                .ToListAsync();

                //Finding all bookings that belong services that this user owns
                var bookingIds = await _context.Bookings
                    .Where(b => b.Service.ServiceProviderId == user.Id)
                    .Select(b => b.Id)
                    .ToListAsync();

                //Finding all services that this user owns
                var serviceIds = await _context.Services
                    .Where(s => s.ServiceProviderId == user.Id)
                    .Select(s => s.Id)
                    .ToListAsync();

                MessageCount = await _context.MessageThreads
                    .Include(mt => mt.AddedBy)
                    .Where(mt =>
                        (bookingIds.Contains(mt.ResourceId) || serviceIds.Contains(mt.ResourceId) || supportTicketIdsAddedByUser.Contains(mt.ResourceId))
                        && mt.ArchivedOn == null).CountAsync();

                AllBookingCount = await _context.Bookings.Where(b => b.Service.ServiceProviderId == user.Id).CountAsync();
                OpenBookingCount = await _context.Bookings.Where(b => b.Service.ServiceProviderId == user.Id && b.Status == BookingStatus.Open).CountAsync();
                ConfirmedBookingCount = await _context.Bookings.Where(b => b.Service.ServiceProviderId == user.Id && b.Status == BookingStatus.Confirm).CountAsync();
                RejectedBookingCount = await _context.Bookings.Where(b => b.Service.ServiceProviderId == user.Id && b.Status == BookingStatus.Decline).CountAsync();
                ReviewCount = await _context.Reviews.Where(r => r.Service.ServiceProviderId == user.Id).CountAsync();
                ServiceCount = serviceIds.Count;
            }
            else
            {
                //Find Support Tickets
                var supportTicketIdsAddedByUser = await _context.SupportTickets
                    .Where(b => b.UserId == user.Id)
                    .Select(b => b.Id)
                    .ToListAsync();

                //Find the bookings added by user
                var bookingIdsAddedByUser = await _context.Bookings
                    .Where(b => b.AddedBy.Id == user.Id)
                    .Select(b => b.Id)
                    .ToListAsync();

                MessageCount = await _context.MessageThreads
                    .Where(mt =>
                        (mt.AddedBy.Id == user.Id || bookingIdsAddedByUser.Contains(mt.ResourceId) || supportTicketIdsAddedByUser.Contains(mt.ResourceId))
                        && mt.ArchivedOn == null).CountAsync();

                AllBookingCount = await _context.Bookings.Where(b => b.AddedById == user.Id).CountAsync();
                OpenBookingCount = await _context.Bookings.Where(b => b.AddedById == user.Id && b.Status == BookingStatus.Open).CountAsync();
                ConfirmedBookingCount = await _context.Bookings.Where(b => b.AddedById == user.Id && b.Status == BookingStatus.Confirm).CountAsync();
                RejectedBookingCount = await _context.Bookings.Where(b => b.AddedById == user.Id && b.Status == BookingStatus.Decline).CountAsync();
                ReviewCount = await _context.Reviews.Where(r => r.AddedById == user.Id).CountAsync();
            }

            return Page();
        }
    }
}
