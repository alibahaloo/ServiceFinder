using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.SupportTickets
{
    [Authorize]
    public class EditModel : BasePageModel
    {
        private readonly SupportTicketRepo _ticketRepo;

        public EditModel(UserManager<Data.ApplicationUser> userManager, IFlashMessage flashMessage, SupportTicketRepo ticketRepo) : base(userManager, flashMessage)
        {
            _ticketRepo = ticketRepo;
        }

        [BindProperty]
        public Data.InputModels.SupportTicket Input { get; set; } = default!;

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

            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Email == null) { return Unauthorized(); }
            //Only owner can edit
            if (!await _ticketRepo.IsUserOwnerAsync(user.Email, supportticket.Id)) { return Unauthorized(); }

            Input = new Data.InputModels.SupportTicket
            {
                Id = supportticket.Id,
                Description = supportticket.Description,
                EmailAddress = supportticket.EmailAddress
            };

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {

            if (!ModelState.IsValid)
            {
                return Page();
            }
            var supportticket = await _ticketRepo.GetSupportTicketById(Input.Id);

            if (supportticket == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Email == null) { return Unauthorized(); }
            //Only owner can edit
            if (!await _ticketRepo.IsUserOwnerAsync(user.Email, supportticket.Id)) { return Unauthorized(); }

            supportticket.Description = Input.Description;

            await _ticketRepo.UpdateEntityAsync(supportticket);
            _flashMessage.Confirmation("Support Ticket Updated Successfully.");
            return RedirectToPage("Details", new { Id = Input.Id });
        }

    }
}
