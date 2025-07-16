using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Admin.Subscriptions
{
    public class EditModel : AdminPageModel
    {
        private readonly SubscriptionRepo _subscriptionRepo;
        private readonly IFlashMessage _flashMessage;
        public EditModel(SubscriptionRepo subscriptionRepo, IFlashMessage flashMessage)
        {
            _subscriptionRepo = subscriptionRepo;
            _flashMessage = flashMessage;
        }

        [BindProperty]
        public Subscription Subscription { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subscription = await _subscriptionRepo.GetEntityAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }
            Subscription = subscription;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            /*
            if (!ModelState.IsValid)
            {
                return Page();
            }
            */
            await _subscriptionRepo.UpdateEntityAsync(Subscription);

            _flashMessage.Confirmation("User Updated Successfully!");

            return RedirectToPage("./Index");
        }
    }
}
