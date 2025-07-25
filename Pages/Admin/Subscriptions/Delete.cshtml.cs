using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Admin.Subscriptions
{
    public class DeleteModel : AdminPageModel
    {
        private readonly SubscriptionRepo _subscriptionRepo;
        private readonly IFlashMessage _flashMessage;

        public DeleteModel(SubscriptionRepo subscriptionRepo, IFlashMessage flashMessage)
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
            else
            {
                Subscription = subscription;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            await _subscriptionRepo.DeleteEntityAsync(id);

            _flashMessage.Confirmation("Item Deleted Successfully!");
            return RedirectToPage("./Index");
        }
    }
}
