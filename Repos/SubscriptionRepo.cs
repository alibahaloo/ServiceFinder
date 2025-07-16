using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Utility;

namespace ServiceFinder.Repos
{
    public class SubscriptionRepo
    {
        private readonly ApplicationDbContext _context;
        public SubscriptionRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task DeleteEntityAsync(int? id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);

            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Subscription?> GetEntityAsync(int? id)
        {
            return await _context.Subscriptions.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task UpdateEntityAsync(Subscription subscription)
        {
            try
            {
                _context.Subscriptions.Update(subscription);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task CreateEntityAsync(Subscription subscription)
        {
            if (_context.Subscriptions == null || subscription == null) { return; }
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
        }

        public IList<Subscription> GetAll()
        {
            return _context.Subscriptions.OrderBy(s => s.Order).ToList();
        }

        public async Task<PagedResult<Subscription>> GetAllAsync(int curPage)
        {
            return await _context.Subscriptions
                .OrderBy(x => x.Order)
                .GetPaged(curPage);
        }

        public async Task<Subscription?> GetFreeSubscriptionAsync()
        {
            return await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == 1);
        }

        public async Task<Subscription?> GetSubscriptionAsync(string name)
        {
            return await _context.Subscriptions.Where(s => s.Name.ToLower().Contains(name.ToLower())).FirstOrDefaultAsync();
        }

        private async Task<int> GetUserFunctionCount(string userId, SubscriptionFunctions function)
        {
            return function switch
            {
                SubscriptionFunctions.SendMessage => await _context.MessageThreads.Where(m => m.AddedById == userId && m.ArchivedOn == null).CountAsync(),
                SubscriptionFunctions.PostService => await _context.Services.Where(s => s.ServiceProviderId == userId).CountAsync(),
                _ => throw new ArgumentException("Invalid function"),
            };
        }

        public async Task<bool> IsUserAllowed(string userId, SubscriptionFunctions function, int serviceId = 0, string? serviceProviderId = null)
        {
            //Get user's subscription first
            var user = await _context.ApplicationUsers.Include(a => a.Subscription).FirstOrDefaultAsync(a => a.Id == userId);
            if (user == null || user.Subscription == null) { throw new InvalidDataException("Subscription is null"); }

            //If user is admin, then bypass all
            if (user.UserType == UserAccountRoles.Admin) return true;


            //Find the relevant resource, if the owner is the current user, then allow it
            switch (function)
            {
                case SubscriptionFunctions.ViewServiceContactInfo:
                    if (serviceId == 0) throw new Exception("serviceId cannot be 0 when function == ViewServiceContactInfo");
                    var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId);
                    if (service != null && service.ServiceProviderId == userId) { return true; }
                    break;
                case SubscriptionFunctions.ViewProviderContactInfo:
                    if (serviceProviderId == null) throw new Exception("serviceProviderId cannot be null when function == ViewProviderContactInfo"); ;
                    var serviceProvider = await _context.ApplicationUsers.FirstOrDefaultAsync(s => s.Id == serviceProviderId);
                    if (serviceProvider != null && serviceProvider.Id == userId) { return true; }
                    break;
            }

            //If value for subscription is null, then it can be unlimited, so all fine
            //otherwise the count should be less that the value
            return function switch
            {
                SubscriptionFunctions.SendMessage => user.Subscription.MaxMessages == null || await GetUserFunctionCount(user.Id, function) < user.Subscription.MaxMessages,
                SubscriptionFunctions.PostService => user.Subscription.MaxServices == null || await GetUserFunctionCount(user.Id, function) < user.Subscription.MaxServices,
                SubscriptionFunctions.ViewServiceContactInfo => user.Subscription.CanViewServiceContactInfo,
                SubscriptionFunctions.ViewProviderContactInfo => user.Subscription.CanViewProviderContactInfo,
                SubscriptionFunctions.BookService => user.Subscription.CanBookService,
                SubscriptionFunctions.WriteReview => user.Subscription.CanWriteReview,
                _ => throw new ArgumentException("Invalid function"),
            };
        }
    }
}
