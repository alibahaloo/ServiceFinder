using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;

namespace ServiceFinder.Pages.Admin.Subscriptions
{
    public class DetailsModel : AdminPageModel
    {
        private readonly SubscriptionRepo _subscriptionRepo;

        public DetailsModel(SubscriptionRepo subscriptionRepo)
        {
            _subscriptionRepo = subscriptionRepo;
        }

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
    }
}
