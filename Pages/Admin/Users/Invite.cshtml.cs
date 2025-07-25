using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using ServiceFinder.Utility;
using Vereyon.Web;

namespace ServiceFinder.Pages.Admin.Users
{
    public class InviteModel : AdminPageModel
    {
        private readonly EmailService _emailSender;
        private readonly IFlashMessage _flashMessage;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly InvitationRepo _invitationRepo;
        public InviteModel(EmailService emailSender, IFlashMessage flashMessage, UserManager<ApplicationUser> userManager, InvitationRepo invitationRepo)
        {
            _emailSender = emailSender;
            _flashMessage = flashMessage;
            _userManager = userManager;
            _invitationRepo = invitationRepo;
        }

        [BindProperty]
        public Data.InputModels.Invitation Invitation { get; set; } = default!;

        public bool Retry { get; set; } = default!;

        public async Task<IActionResult> OnPostRetryAsync()
        {

            //Send email
            var result = _emailSender.SendInvitation(Invitation.EmailAddress);
            //Increment retry count
            await _invitationRepo.RetryAsync(Invitation.Id);

            switch (result.Item1)
            {
                case true:
                    _flashMessage.Confirmation(result.Item2);
                    break;
                case false:
                    _flashMessage.Danger(result.Item2);
                    break;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //Check if a user with this email already exists.
            var user = await _userManager.FindByEmailAsync(Invitation.EmailAddress);
            if (user != null)
            {
                _flashMessage.Danger("There's already a user registered with this email address.");
                return RedirectToPage("./Index");
            }

            //if not, check we've already sent an invitation
            var existingInvitationId = await _invitationRepo.FindByEmail(Invitation.EmailAddress);

            if (existingInvitationId != 0)
            {
                //ask the admin if they want to retry
                return RedirectToPage("./Invite", new { retry = true, Id = existingInvitationId });
            }

            var result = _emailSender.SendInvitation(Invitation.EmailAddress);

            var newInvitation = new Data.Invitation
            {
                EmailAddress = Invitation.EmailAddress
            };

            await _invitationRepo.CreateEntityAsync(newInvitation);

            switch (result.Item1)
            {
                case true:
                    _flashMessage.Confirmation(result.Item2);
                    break;
                case false:
                    _flashMessage.Danger(result.Item2);
                    break;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGet(bool? retry, int? Id)
        {
            Retry = retry != null;

            if (Id != null)
            {
                var invitation = await _invitationRepo.GetEntityAsync(Id);

                if (invitation == null) { return NotFound(); }

                Invitation = new Data.InputModels.Invitation
                {
                    Id = invitation.Id,
                    EmailAddress = invitation.EmailAddress,
                    RetryCount = invitation.RetryCount,
                };
            }

            return Page();
        }
    }
}
