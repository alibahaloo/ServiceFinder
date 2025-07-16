using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using ServiceFinder.Utility;
using Vereyon.Web;

namespace ServiceFinder.Pages.Bookings
{
    [Authorize]
    public class DeleteModel : BasePageModel
    {
        private readonly BookingRepo _bookingRepo;
        private readonly ServiceRepo _serviceRepo;
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _context;

        public DeleteModel(BookingRepo bookingRepo, IFlashMessage flashMessage, UserManager<ApplicationUser> userManager, ServiceRepo serviceRepo, EmailService emailService, ApplicationDbContext context)
        : base(userManager, flashMessage)
        {
            _bookingRepo = bookingRepo;
            _emailService = emailService;
            _serviceRepo = serviceRepo;
            _context = context;
        }

        [BindProperty]
        public Booking Booking { get; set; } = default!;
        public Data.MessageThread? MessageThread { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _bookingRepo.GetEntityAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            //Check ownership
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Unauthorized(); }

            if (booking.IsExpired)
            {
                //Service owner and booking owner and admin can delete
                if (user.UserType == UserAccountRoles.Customer && !await _bookingRepo.IsUserOwnerAsync(user.Id, booking.Id)) { return Unauthorized(); }
            }
            else
            {
                //only owner can delete
                if (!await _bookingRepo.IsUserOwnerAsync(user.Id, booking.Id)) { return Unauthorized(); }
            }

            Booking = booking;
            //It's safe to do this since bookings are unique for users, thus we can look for a thread without explicitly having its ID
            MessageThread = await _context.MessageThreads.Where(mt => mt.ResourceId == booking.Id && mt.ResourceType == MessageResourceType.Booking && mt.ArchivedOn == null).FirstOrDefaultAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _bookingRepo.GetEntityAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            //Check ownership
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Unauthorized(); }

            if (booking.IsExpired)
            {
                //Service owner and booking owner and admin can delete
                if (user.UserType == UserAccountRoles.Customer && !await _bookingRepo.IsUserOwnerAsync(user.Id, booking.Id)) { return Unauthorized(); }
            } else
            {
                //only owner can delete
                if (!await _bookingRepo.IsUserOwnerAsync(user.Id, booking.Id)) { return Unauthorized(); }
            }

            await _bookingRepo.DeleteEntityAsync(id);
            //TODO Delete messages

            _flashMessage.Confirmation("Item Deleted Successfully!");
            return RedirectToPage("./MyBookings");
        }
    }
}
