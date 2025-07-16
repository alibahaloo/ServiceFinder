using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Admin.Subscriptions
{
    public class IndexModel : AdminPageModel
    {
        private readonly SubscriptionRepo _subscriptionRepo;
        private readonly ApplicationDbContext _context;
        public readonly IFlashMessage _flashMessage;

        public int CurPage { get; set; }
        public int PageCount { get; set; }
        public IndexModel(SubscriptionRepo subscriptionRepo, ApplicationDbContext context, IFlashMessage flashMessage)
        {
            _subscriptionRepo = subscriptionRepo;
            _context = context;
            _flashMessage = flashMessage;
        }

        public IList<Subscription> Subscriptions { get; set; } = default!;

        public async Task<IActionResult> OnPostSaveOrderAsync(string listOrder)
        {
            // Your logic to order ingredients based on the dictionary
            Dictionary<int, int>? listOrderDictionary = null;

            try
            {
                // Deserialize JSON string to Dictionary<int, int>
                listOrderDictionary = JsonConvert.DeserializeObject<Dictionary<int, int>>(listOrder);
            }
            catch (Exception)
            {

                throw;
            }

            if (listOrderDictionary != null)
            {
                //Do stuff here
                foreach (var item in listOrderDictionary)
                {
                    var subscription = await _context.Subscriptions.FirstOrDefaultAsync(m => m.Id == item.Key);

                    if (subscription != null)
                    {
                        subscription.Order = item.Value;
                    }
                }
            }

            await _context.SaveChangesAsync();
            string referer = Request.Headers.Referer.ToString();
            _flashMessage.Confirmation("Order saved successfully.");
            return Redirect(referer);
        }

        public async Task OnGetAsync(int curPage = 1)
        {
            CurPage = curPage;

            var result = await _subscriptionRepo.GetAllAsync(CurPage);
            Subscriptions = result.Data;

            PageCount = result.PageCount;
        }
    }
}
