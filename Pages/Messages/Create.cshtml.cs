using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Data.InputModels;
using ServiceFinder.Repos;
using ServiceFinder.Utility;
using Vereyon.Web;

namespace ServiceFinder.Pages.Messages
{
    [Authorize]
    public class CreateModel : BasePageModel
    {
        private readonly ServiceRepo _serviceRepo;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly NotificationRepo _notificationRepo;

        public Data.MessageThread? MessageThread { get; set; } = default!;

        [BindProperty]
        public Data.InputModels.Message Input { get; set; } = default!;
        public Data.Service Service { get; set; } = default!;
        public CreateModel(IFlashMessage flashMessage, ServiceRepo serviceRepo, UserManager<Data.ApplicationUser> userManager, ApplicationDbContext context, EmailService emailService, NotificationRepo notificationRepo)
       : base(userManager, flashMessage)
        {
            _serviceRepo = serviceRepo;
            _context = context;
            _emailService = emailService;
            _notificationRepo = notificationRepo;
        }

        public async Task<IActionResult> OnGetAsync(int serviceId, int? threadId = null)
        {
            if (serviceId == 0)
            {
                return NotFound();
            }

            var service = await _serviceRepo.GetEntityAsync(serviceId);

            if (service == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            //Check if the user is a service provider (an an owner)
            if (user.UserType == UserAccountRoles.ServiceProvider && !await _serviceRepo.IsUserOwnerAsync(user.Id, serviceId))
            {
                _flashMessage.Danger("You cannot use this functionality when logged in as a 'Service Provider'. Log in as a 'Customer' to be able to do so.");
                return Redirect("/Services");
            }


            if (threadId == null)
            {
                //We get here if user clicks on "send message" button from the home page
                //so we check if the user has a an open convo
                var messageThread = await _context.MessageThreads
                    .Where(mt => mt.ResourceType == MessageResourceType.Service
                    && mt.ResourceId == serviceId
                    && mt.AddedById == user.Id
                    && mt.ArchivedOn == null).FirstOrDefaultAsync();
                if (messageThread != null)
                {
                    //and if so, we will assign the threadId
                    threadId = messageThread.Id;
                }
            }

            Service = service;
            MessageThread = await _context.MessageThreads.FindAsync(threadId);

            Input = new() { ResourceId = serviceId, ResourceType = MessageResourceType.Service, ThreadId = MessageThread?.Id };

            //Mark any new Messages are read (Delete the alert)
            var alerts = await _context.NewMessageThreadAlerts
                .Where(alert => alert.MessageThreadId == threadId && alert.ApplicationUserId == user.Id)
                .ToListAsync();

            if (alerts.Count != 0)
            {
                // Remove the records
                _context.NewMessageThreadAlerts.RemoveRange(alerts);
                // Save changes
                await _context.SaveChangesAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSendMessageAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var service = await _serviceRepo.GetEntityAsync(Input.ResourceId);

            if (service == null)
            {
                return NotFound();
            }

            MessageThread = await _context.MessageThreads.Include( mt => mt.AddedBy).FirstOrDefaultAsync( mt => mt.Id == Input.ThreadId);

            if (Input.Body == null || Input.Body.Length == 0)
            {
                _flashMessage.Warning("Message body cannot be empty!");
                return Redirect($"../Messages/Create?serviceId={Input.ResourceId}&threadId={MessageThread?.Id}");
            }

            //check if thread exists, if not create it
            if (MessageThread == null)
            {
                MessageThread = new() { ResourceId = Input.ResourceId, ResourceType = MessageResourceType.Service };
                _context.MessageThreads.Add(MessageThread);
            }

            //Now save the message in the thread
            var message = new Data.Message { Body = Input.Body, MessageThread = MessageThread };
            _context.Messages.Add(message);

            //Prepare to send email and alert
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
            
            //Finally save everything
            await _context.SaveChangesAsync();

            //Attempt to send email
            if (alertUser.Email != null && await _notificationRepo.GetUserNotificationStatusByFunction(NotificationFunction.MessageReceived, alertUser.Id))
                _emailService.SendMessageNotification($"/Messages/Create?serviceId={Input.ResourceId}&threadId={MessageThread.Id}", alertUser.Email);

            _flashMessage.Confirmation("Message Sent Successfully!");
            return Redirect($"../Messages/Create?serviceId={Input.ResourceId}&threadId={MessageThread.Id}");
        }
    }
}
