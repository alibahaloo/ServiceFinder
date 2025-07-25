using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using ServiceFinder.Utility;
using Vereyon.Web;

namespace ServiceFinder.Pages.SupportTickets
{
    [Authorize]
    public class CreateModel : BasePageModel
    {
        private readonly SupportTicketRepo _SupportTicketRepo;
        private readonly EmailService _emailService;
        private readonly NotificationRepo _notificationRepo;
        public CreateModel(UserManager<Data.ApplicationUser> userManager, SupportTicketRepo supportTicketRepo, IFlashMessage flashMessage, EmailService emailService, NotificationRepo notificationRepo)
            : base(userManager, flashMessage)
        {
            _SupportTicketRepo = supportTicketRepo;
            _emailService = emailService;
            _notificationRepo = notificationRepo;
        }

        public async Task<IActionResult> OnGet()
        {
            CurUser = await _userManager.GetUserAsync(User);
            if (CurUser != null && CurUser.Email != null)
            {
                SupportTicket = await _SupportTicketRepo.GetSupportTicketByEmail(CurUser.Email);
            }
            return Page();
        }

        [BindProperty]
        public Data.InputModels.SupportTicket Input { get; set; } = default!;

        public Data.SupportTicket? SupportTicket { get; set; } = default!;
        public Data.ApplicationUser? CurUser { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.Description == null)
            {
                _flashMessage.Warning("Message body cannot be empty!");
                return RedirectToPage();
            }

            if (Input.EmailAddress == null || Input.EmailAddress.Length < 0)
            {
                _flashMessage.Warning("Email field cannot be empty!");
                return RedirectToPage();
            }

            var user = await _userManager.FindByEmailAsync(Input.EmailAddress);
            if (user == null)
            {
                //This will prevent non-registered users to create support ticket
                TempData["ErrorMessage"] = $"Unable to find user with email '{Input.EmailAddress}'. You need to be a registered user in order submit a support ticket.";
                return BadRequest();
            }

            //Check if there's already a support ticket for this use
            if (await _SupportTicketRepo.GetSupportTicketByEmail(Input.EmailAddress) != null)
            {
                TempData["ErrorMessage"] = "There is already a support ticket associated with this email address.";
                return BadRequest();
            }

            var supportTicket = new Data.SupportTicket
            {
                EmailAddress = Input.EmailAddress,
                Description = Input.Description,
                Status = SupportTicketStatus.Open,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
            };
            //Create Support Ticket
            int supportTicketId = await _SupportTicketRepo.CreateEntityAsync(supportTicket);

            //Send email to user AND to admin team
            if (user.Email != null)
            {
                if (await _notificationRepo.GetUserNotificationStatusByFunction(NotificationFunction.SupportTicketCreated, user.Id))
                {
                    _emailService.SendSupportTicketCreated($"/SupportTickets/Details?Id={supportTicketId}", user.Email);

                }
                _emailService.SendSupportTicketCreated($"/SupportTickets/Details?Id={supportTicketId}");
            }

            _flashMessage.Confirmation("Support ticket successfully created. You will be notified via email once a admin team reviews and replies.");
            return Redirect($"/SupportTickets/Details?Id={supportTicketId}");
        }
    }
}
