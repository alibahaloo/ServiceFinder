using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using ServiceFinder.Utility;
using Vereyon.Web;

namespace ServiceFinder.Pages.SupportTickets
{
    [Authorize]
    public class DetailsModel : BasePageModel
    {
        private readonly SupportTicketRepo _ticketRepo;
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _context;
        private readonly NotificationRepo _notificationRepo;


        public DetailsModel(UserManager<Data.ApplicationUser> userManager, IFlashMessage flashMessage, SupportTicketRepo ticketRepo, EmailService emailService, ApplicationDbContext context, NotificationRepo notificationRepo)
        : base(userManager, flashMessage)
        {
            _ticketRepo = ticketRepo;
            _emailService = emailService;
            _context = context;
            _notificationRepo = notificationRepo;
        }

        public Data.SupportTicket SupportTicket { get; set; } = default!;
        public Data.MessageThread? MessageThread { get; set; } = default!;
        [BindProperty]
        public Data.InputModels.Message Input { get; set; } = default!;

        public async Task<IActionResult> OnPostSendMessageAsync()
        {
            //Check if there's a body
            if (Input.Body == null)
            {
                _flashMessage.Warning("Message body cannot be empty!");
                return RedirectToPage(new { Id = Input.ResourceId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Email == null) { return Unauthorized(); }

            var supportTicket = await _ticketRepo.GetSupportTicketById(Input.ResourceId);
            if (supportTicket == null)
            {
                return NotFound();
            }

            MessageThread = await _context.MessageThreads.FindAsync(Input.ThreadId);

            //check if thread exists, if not create it
            if (MessageThread == null)
            {
                MessageThread = new() { ResourceId = Input.ResourceId, ResourceType = MessageResourceType.SupportTicket };
                _context.MessageThreads.Add(MessageThread);
                await _context.SaveChangesAsync();
            }

            //Now save the message in the thread
            var message = new Data.Message { Body = Input.Body, MessageThreadId = MessageThread.Id };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            //send an email to user
            //check if the user is the booking owner, or the service provider
            if (user.Id == supportTicket.UserId)
            {
                //It's the owner replying
                _emailService.SendMessageNotification($"/SupportTickets/Details?Id={Input.ResourceId}");
            }
            else
            {
                //It has to be the admins
                if (supportTicket.EmailAddress != null) _emailService.SendMessageNotification($"/SupportTickets/Details?Id={Input.ResourceId}",supportTicket.EmailAddress);
            }

            _flashMessage.Confirmation("Message Sent Successfully!");
            return RedirectToPage(new { Id = Input.ResourceId });
        }

        public async Task<IActionResult> OnPostAsync(int id, SupportTicketStatus status)
        {
            var supportticket = await _ticketRepo.GetSupportTicketById(id);

            if (supportticket == null)
            {
                return NotFound();
            }

            //Check ownership
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Email == null) { return Unauthorized(); }


            switch (user.UserType)
            {
                case not UserAccountRoles.Admin:
                    if (!await _ticketRepo.IsUserOwnerAsync(user.Email, supportticket.Id)) { return Unauthorized(); }
                    break;
            }

            supportticket.Status = status;

            await _ticketRepo.UpdateEntityAsync(supportticket);

            //email update status to user
            if (supportticket.EmailAddress != null && await _notificationRepo.GetUserNotificationStatusByFunction(NotificationFunction.SupportTicketUpdated, supportticket.UserId))
                _emailService.SendSupportTicketUpdated(supportticket.EmailAddress, $"/SupportTickets/Details?Id={supportticket.Id}");

            _flashMessage.Confirmation("Status Updated Successfully!");
            return RedirectToPage(new { Id = supportticket.Id });
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supportticket = await _ticketRepo.GetSupportTicketById(id);

            if (supportticket == null)
            {
                return NotFound();
            }

            //Check ownership .. only admin can view, and the owner of the ticket
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Email == null) { return Unauthorized(); }

            SupportTicket = supportticket;
            //It's safe to do this since support tickets are unique for users, thus we can look for a thread without explicitly having its ID
            MessageThread = await _context.MessageThreads.Where(mt => mt.ResourceId == supportticket.Id && mt.ResourceType == MessageResourceType.SupportTicket && mt.ArchivedOn == null).FirstOrDefaultAsync();

            Input = new() { ResourceId = supportticket.Id, ResourceType = MessageResourceType.Booking, ThreadId = MessageThread?.Id };


            return Page();
        }
    }
}
