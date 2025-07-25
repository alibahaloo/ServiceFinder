// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using ServiceFinder.Utility;
using Vereyon.Web;

namespace ServiceFinder.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<Data.ApplicationUser> _userManager;
        private readonly SupportTicketRepo _SupportTicketRepo;
        private readonly IFlashMessage _flashMessage;
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _context;

        public RegisterConfirmationModel(UserManager<Data.ApplicationUser> userManager, SupportTicketRepo supportTicketRepo, IFlashMessage flashMessage, EmailService emailService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _SupportTicketRepo = supportTicketRepo;
            _flashMessage = flashMessage;
            _emailService = emailService;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }
        [BindProperty]
        public ServiceProviderType ServiceProviderType { get; set; } = ServiceProviderType.Freelancer;
        [BindProperty]
        public Data.InputModels.SupportTicket Input { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.Description == null || Input.Description.Length < 0)
            {
                _flashMessage.Warning("Description field cannot be empty!");
                return RedirectToPage(new { email = Input.EmailAddress });
            }

            var user = await _userManager.FindByEmailAsync(Input.EmailAddress);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{Input.EmailAddress}'.");
            }

            //Only service providers can do this
            if (user.UserType != UserAccountRoles.ServiceProvider)
            {
                return Unauthorized();
            }

            //Check if there's already a support ticket for this use
            if (await _SupportTicketRepo.GetSupportTicketByEmail(Input.EmailAddress) != null)
            {
                _flashMessage.Danger("There is already a support ticket associated with this email address.");
                return BadRequest();
            }

            var userProfile = await _context.ApplicationUsers.FirstOrDefaultAsync(i => i.Id == user.Id);
            if (userProfile != null)
            {
                userProfile.ServiceProviderType = ServiceProviderType;
                _context.ApplicationUsers.Update(userProfile);
                await _context.SaveChangesAsync();
            }

            var supportTicket = new Data.SupportTicket
            {
                EmailAddress = Input.EmailAddress,
                Description = Input.Description,
                Status = SupportTicketStatus.Open,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IsAccessRequest = true,
            };
            //Create Support Ticket
            var newId = await _SupportTicketRepo.CreateEntityAsync(supportTicket);
            //Send an email to user
            _emailService.SendAccountAccessRequest(user.Email);

            //Send an email to support
            _emailService.SendSupportTicketCreated($"/SupportTickets/Details?Id={newId}");

            _flashMessage.Confirmation("Access request submitted successfully. You will be notified via email once a decision is made.");
            return Redirect($"/SupportTickets/Details?Id={newId}");
        }

        public async Task<IActionResult> OnGetAsync(string email)
        {

            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            //Check if there's already a support ticket for this use
            var existingSupportTicket = await _SupportTicketRepo.GetSupportTicketByEmail(email);
            if (existingSupportTicket != null)
            {
                _flashMessage.Warning("You already have submitted your access request. You can see the details below and chat with our support team using the messaging system.");
                return Redirect($"/SupportTickets/Details?id={existingSupportTicket.Id}");
            }

            //Check if the user is already confirmed and verified
            if (user.IsVerified)
            {
                return NotFound($"The user with '{email}' is already verified.");
            }

            Email = email;

            Input = new Data.InputModels.SupportTicket
            {
                Description = string.Empty,
                EmailAddress = user.Email,
            };

            return Page();
        }
    }
}
