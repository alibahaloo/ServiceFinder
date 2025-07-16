using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Utility;

namespace ServiceFinder.Repos
{
    public class ServiceRepo
    {
        private readonly ApplicationDbContext _context;
        public ServiceRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        //Check if the given user is the owner of the given service
        public async Task<bool> IsUserOwnerAsync(string userId, int serviceId)
        {
            var service = await _context.Services.FirstOrDefaultAsync(m => m.Id == serviceId);

            if (service == null)
            {
                return false;
            }

            return (service.ServiceProviderId == userId);
        }

        public async Task<IList<Service>> GetAllAsync()
        {
            return await _context.Services.Include(s => s.ServiceProvider).Where(s => s.ServiceProvider.IsVerified == true).ToListAsync();
        }

        public async Task<IList<Data.Service>> GetProviderServicesAsync(string providerId)
        {
            return await _context.Services
                    .Include(s => s.ServiceCategories)
                    .ThenInclude(x => x.Category)
                    .Include(s => s.ServiceCities)
                    .ThenInclude(sc => sc.City)
                    .Include(s => s.ServiceProvider)
                    .Include(s => s.City)
                    .ThenInclude(c => c.Province)
                    .Where(s => s.ServiceProviderId == providerId)
                    .OrderBy(s => s.Id)
                    .ToListAsync();
        }

        public IQueryable<Data.Service> GetSearchQuery()
        {
            return _context.Services
                .Include(s => s.ServiceCategories)
                .ThenInclude(x => x.Category)
                .Include(s => s.ServiceCities)
                .ThenInclude(sc => sc.City)
                .Include(s => s.City)
                .ThenInclude(c => c.Province)
                .Include(s => s.ServiceProvider)
                .OrderBy(s => s.Id);
        }

        public async Task<PagedResult<Service>> SearchQuery(int curPage, IQueryable<Data.Service> query)
        {

            return await query
                .OrderBy(s => s.Id)
                .GetPaged(curPage);
        }

        public async Task<Service?> GetEntityAsync(int? id)
        {
            return await _context.Services
                .Include(s => s.ServiceImages.OrderBy(si => si.Order))
                .Include(s => s.Reviews)
                .ThenInclude(sr => sr.AddedBy)
                .Include(s => s.ServiceCategories)
                .ThenInclude(x => x.Category)
                .Include(s => s.ServiceCities)
                .ThenInclude(sc => sc.City)
                .Include(m => m.City)
                .ThenInclude(m => m.Province)
                .Include(m => m.ServiceProvider)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task UpdateEntityAsync(Service service, List<int> selectedCitiesIds, List<int> selectedCategoryIds)
        {
            try
            {
                var entity = await GetEntityAsync(service.Id);

                if (entity != null)
                {
                    entity.Name = service.Name;
                    entity.Description = service.Description.Replace("\n", ", ");
                    entity.AddressLine1 = service.AddressLine1;
                    entity.AddressLine2 = service.AddressLine2;
                    entity.PhoneNumber = service.PhoneNumber;
                    entity.PostalCode = service.PostalCode.ToUpper();

                    //Service Categories
                    if (entity.ServiceCategories != null)
                    {
                        entity.ServiceCategories.Clear();

                        foreach (var item in selectedCategoryIds)
                        {
                            var category = await _context.Categories.FirstOrDefaultAsync(s => s.Id == item);
                            if (category != null)
                            {
                                var serviceCategory = new ServiceCategory { Category = category, Service = service };
                                entity.ServiceCategories.Add(serviceCategory);
                            }
                        }
                    }

                    //Cities
                    if (entity.ServiceCities != null)
                    {
                        entity.ServiceCities.Clear();

                        foreach (var item in selectedCitiesIds)
                        {
                            var city = await _context.Cities.FirstOrDefaultAsync(c => c.Id == item);
                            if (city != null)
                            {
                                var serviceCity = new ServiceCity { City = city, Service = service };
                                entity.ServiceCities.Add(serviceCity);
                            }
                        }
                    }

                    _context.Services.Update(entity);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task CreateEntityAsync(Service service, List<int> selectedCitiesIds, List<int> selectedCategoryIds)
        {
            if (_context.Services == null || service == null) { return; }

            foreach (var item in selectedCitiesIds)
            {
                var city = await _context.Cities.FirstOrDefaultAsync(c => c.Id == item);
                if (city != null)
                {
                    var serviceCity = new ServiceCity { City = city, Service = service };
                    service.ServiceCities.Add(serviceCity);
                }
            }

            foreach (var item in selectedCategoryIds)
            {
                var category = await _context.Categories.FirstOrDefaultAsync(s => s.Id == item);
                if (category != null)
                {
                    var serviceCategory = new ServiceCategory { Category = category, Service = service };
                    service.ServiceCategories.Add(serviceCategory);
                }
            }

            service.Description = service.Description.Replace("\n", ", ");
            service.PostalCode = service.PostalCode.ToUpper();

            _context.Services.Add(service);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEntityAsync(int? id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service != null)
            {
                //Delete all comms for this booking
                var comms = await _context.MessageThreads.Where(c => c.ResourceId == service.Id && c.ResourceType == MessageResourceType.Service).ToListAsync();
                _context.MessageThreads.RemoveRange(comms);

                //Remove all reviews related to service
                var serviceReviews = await _context.Reviews.Where(r => r.ServiceId == service.Id).ToListAsync();
                _context.Reviews.RemoveRange(serviceReviews);

                //Remove all bookings related to service
                var serviceBookings = await _context.Bookings.Where(b => b.ServiceId == service.Id).ToListAsync();
                if (serviceBookings.Count > 0)
                {
                    //Remove comms for each booking
                    foreach (var booking in serviceBookings)
                    {
                        var bookingComms = await _context.MessageThreads.Where(c => c.ResourceId == booking.Id && c.ResourceType == MessageResourceType.Booking).ToListAsync();
                        _context.MessageThreads.RemoveRange(bookingComms);
                    }

                    _context.Bookings.RemoveRange(serviceBookings);
                }

                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }
    }
}
