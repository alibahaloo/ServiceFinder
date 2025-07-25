using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using ServiceFinder.Utility;
using System.Threading;
using Vereyon.Web;

namespace ServiceFinder.Pages.Bookings
{
    [Authorize]
    public class DetailsModel : BasePageModel
    {
        private readonly BookingRepo _bookingRepo;
        private readonly EmailService _emailService;
        private readonly ServiceRepo _serviceRepo;
        private readonly ApplicationDbContext _context;
        private readonly NotificationRepo _notificationRepo;

        public DetailsModel(BookingRepo bookingRepo, UserManager<Data.ApplicationUser> userManager, IFlashMessage flashMessage, EmailService emailService, ServiceRepo serviceRepo, ApplicationDbContext context, NotificationRepo notificationRepo)
        : base(userManager, flashMessage)
        {
            _bookingRepo = bookingRepo;
            _emailService = emailService;
            _serviceRepo = serviceRepo;
            _context = context;
            _notificationRepo = notificationRepo;
        }

        [BindProperty]
        public Data.Booking Booking { get; set; } = default!;
        public Data.MessageThread? MessageThread { get; set; } = default!;
        [BindProperty]
        public Data.InputModels.Message Input { get; set; } = default!;

        public async Task<IActionResult> OnPostSendMessageAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            //Check if there's a body
            if (Input.Body == null)
            {
                _flashMessage.Warning("Message body cannot be empty!");
                return RedirectToPage(new { Id = Input.ResourceId });
            }

            var booking = await _bookingRepo.GetEntityAsync(Input.ResourceId);
            if (booking == null)
            {
                return NotFound();
            }

            var service = await _serviceRepo.GetEntityAsync(booking.ServiceId);
            if (service == null)
            {
                return NotFound();
            }

            MessageThread = await _context.MessageThreads.FindAsync(Input.ThreadId);

            //check if thread exists, if not create it
            if (MessageThread == null)
            {
                MessageThread = new() { ResourceId = Input.ResourceId, ResourceType = MessageResourceType.Booking };
                _context.MessageThreads.Add(MessageThread);
            }

            //Now save the message in the thread
            var message = new Data.Message { Body = Input.Body, MessageThreadId = MessageThread.Id };
            _context.Messages.Add(message);

            Data.ApplicationUser alertUser;

            //Check if the user is the service owner or a customer
            if (user.UserType == UserAccountRoles.ServiceProvider)
            {
                //it's the provider replying
                alertUser = MessageThread.AddedBy;
            }
            else
            {
                //it's the customer replying
                alertUser = service.ServiceProvider;
            }

            //Save alert for new message
            if (!await _context.NewMessageThreadAlerts.AnyAsync(alert => alert.MessageThreadId == MessageThread.Id && alert.ApplicationUserId == alertUser.Id))
            {
                var newMessageThreadAlert = new NewMessageThreadAlert { MessageThread = MessageThread, ApplicationUser = alertUser };
                _context.NewMessageThreadAlerts.Add(newMessageThreadAlert);
            }

            await _context.SaveChangesAsync();

            //Attempt to send email
            if (alertUser.Email != null && await _notificationRepo.GetUserNotificationStatusByFunction(NotificationFunction.MessageReceived, alertUser.Id))
                _emailService.SendMessageNotification($"/Bookings/Details?Id={Input.ResourceId}", alertUser.Email);

            
            _flashMessage.Confirmation("Message Sent Successfully!");
            return RedirectToPage(new { Id = Input.ResourceId });
        }

        public async Task<IActionResult> OnPostAsync(int id, BookingStatus status)
        {
            var booking = await _bookingRepo.GetEntityAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            //Check ownership
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Unauthorized(); }

            switch (user.UserType)
            {
                case UserAccountRoles.ServiceProvider:
                    if (booking.Service.ServiceProviderId != user.Id) return Unauthorized();
                    break;
                default:
                    if (booking.AddedById != user.Id) return Unauthorized();
                    break;
            }

            booking.Status = status;

            await _bookingRepo.UpdateEntityAsync(booking);

            if (booking.AddedBy.Email != null) _emailService.SendBookingStatusUpdated(booking.AddedBy.Email, $"/Bookings/Details?Id={booking.Id}", status.ToString());

            _flashMessage.Confirmation("Item Updated Successfully!");

            string refererUrl = Request.Headers["Referer"].ToString();
            return Redirect(refererUrl);
        }

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

            Booking = booking;
            //It's safe to do this since bookings are unique for users, thus we can look for a thread without explicitly having its ID
            MessageThread = await _context.MessageThreads.Where(mt => mt.ResourceId == booking.Id && mt.ResourceType == MessageResourceType.Booking && mt.ArchivedOn == null).FirstOrDefaultAsync();

            Input = new() { ResourceId = booking.Id, ResourceType = MessageResourceType.Booking, ThreadId = MessageThread?.Id };

            if (MessageThread?.Id != null)
            {

                //Mark any new Messages are read (Delete the alert)
                var alerts = await _context.NewMessageThreadAlerts
                    .Where(alert => alert.MessageThreadId == MessageThread.Id && alert.ApplicationUserId == user.Id)
                    .ToListAsync();

                if (alerts.Count != 0)
                {
                    // Remove the records
                    _context.NewMessageThreadAlerts.RemoveRange(alerts);
                    // Save changes
                    await _context.SaveChangesAsync();
                }
            }
            return Page();
        }
    }
}
