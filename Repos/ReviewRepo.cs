using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Utility;

namespace ServiceFinder.Repos
{
    public class ReviewRepo
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUserOwner(string userId, int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null) { return false; }

            return (review.AddedById == userId);
        }

        public async Task<double> GetAverageRatingForProviderAsync(string providerId)
        {
            var averageStars = await _context.Reviews
            .Where(r => r.Service.ServiceProviderId == providerId)
            .Select(r => r.Stars)
            .ToListAsync();

            if (averageStars.Any())
            {
                return averageStars.Average();
            }

            return 0; // or any default value when there are no reviews
        }

        public async Task<double> GetAverageRatingForServiceAsync(int serviceId)
        {
            var averageStars = await _context.Reviews
            .Where(r => r.ServiceId == serviceId)
            .Select(r => r.Stars)
            .ToListAsync();

            if (averageStars.Any())
            {
                return averageStars.Average();
            }

            return 0; // or any default value when there are no reviews
        }

        public async Task<int> GetCountByProviderAsync(string providerId)
        {
            return await _context.Reviews
               .Where(r => r.Service.ServiceProviderId == providerId).CountAsync();
        }

        public async Task<int> GetCountByServiceAsync(int serviceId)
        {
            return await _context.Reviews
                .Where(r => r.ServiceId == serviceId).CountAsync();
        }

        public async Task<PagedResult<Review>> GetAllByServiceAsync(int curPage, int serviceId, int count = 12)
        {
            return await _context.Reviews
                .Where(r => r.ServiceId == serviceId)
                .Include(r => r.Service)
                .Include(r => r.AddedBy)
                .OrderByDescending(r => r.CreatedAt)
                .GetPaged(curPage, count);
        }

        public async Task<PagedResult<Review>> GetAllByProviderAsync(int curPage, string serviceProviderId)
        {
            return await _context.Reviews
                .Include(r => r.Service)
                .Include(r => r.AddedBy)
                .Where(r => r.Service.ServiceProviderId == serviceProviderId)
                .OrderByDescending(r => r.CreatedAt)
                .GetPaged(curPage, 12);
        }

        public async Task<PagedResult<Review>> GetAllBySenderAsync(int curPage, string senderId)
        {
            return await _context.Reviews
                .Where(r => r.AddedById == senderId)
                .Include(r => r.Service)
                .Include(r => r.AddedBy)
                .GetPaged(curPage, 12);
        }

        public async Task<Review?> CheckExistingReviewByUser(string userId, int serviceId)
        {
            return await _context.Reviews
                .Where(r => r.ServiceId == serviceId && r.AddedById == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<Review?> GetEntityAsync(int? id)
        {
            return await _context.Reviews
                .Include(r => r.Service)
                .Include(r => r.AddedBy)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task DeleteEntityAsync(int? id)
        {
            var entity = await _context.Reviews.FindAsync(id);

            if (entity != null)
            {
                _context.Reviews.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateEntityAsync(Review entity)
        {
            try
            {
                _context.Reviews.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int?> CreateEntityAsync(Review entity)
        {
            if (_context.Reviews == null || entity == null) { return null; }
            _context.Reviews.Add(entity);
            await _context.SaveChangesAsync();

            return entity.Id;
        }
    }
}
