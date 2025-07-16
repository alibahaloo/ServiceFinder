using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Admin.Subscriptions
{
    public class CreateModel : AdminPageModel
    {
        private readonly SubscriptionRepo _subscriptionRepo;
        private readonly IFlashMessage _flashMessage;


        public CreateModel(SubscriptionRepo subscriptionRepo, IFlashMessage flashMessage)
        {
            _subscriptionRepo = subscriptionRepo;
            _flashMessage = flashMessage;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Subscription Subscription { get; set; } = default!;


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _subscriptionRepo.CreateEntityAsync(Subscription);
            _flashMessage.Confirmation("Item Created Successfully!");

            return RedirectToPage("./Index");
        }
    }
}
