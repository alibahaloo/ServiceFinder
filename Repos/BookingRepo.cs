using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Utility;
using System.Globalization;

namespace ServiceFinder.Repos
{
    public class BookingRepo
    {
        private readonly ApplicationDbContext _context;

        public BookingRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUserOwnerAsync(string userId, int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null) { return false; }

            return (booking.AddedById == userId);
        }

        public async Task<bool> IsDateAlreadyBooked(int serviceId, DateOnly date)
        {
            return await _context.Bookings
                .Where(b => b.ServiceId == serviceId && b.StartDate == date).AnyAsync();
        }

        public async Task<Data.Booking?> GetExistingBookingAsync(int serviceId, string userId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.ServiceId == serviceId && b.AddedById == userId && b.Status == BookingStatus.Open).ToListAsync();

            return bookings.Where(b => !b.IsExpired).FirstOrDefault();
        }

        public async Task<Booking?> GetActiveBookingAsync(int serviceId, string userId)
        {
            return await _context.Bookings.Where(b => b.ServiceId == serviceId && b.AddedById == userId && b.Status == BookingStatus.Open).FirstOrDefaultAsync();
        }

        public async Task<int> GetCountByProviderAsync(string providerId, BookingStatus status)
        {
            return await _context.Bookings
               .Where(b => b.Service.ServiceProviderId == providerId && b.Status == status).CountAsync();
        }

        public async Task<int> GetCountByServiceAsync(int serviceId, BookingStatus status)
        {
            return await _context.Bookings
                .Where(b => b.ServiceId == serviceId && b.Status == status).CountAsync();
        }

        public async Task<IList<DateOnly>> GetAllBookedDatesAsync(int serviceId, string targetMonth, int year)
        {
            // Convert the targetMonth to a month number
            int targetMonthNumber = DateTime.ParseExact(targetMonth, "MMMM", CultureInfo.InvariantCulture).Month;

            // Calculate the start and end dates for the target month and year
            DateOnly startDate = new DateOnly(year, targetMonthNumber, 1);
            DateOnly endDate = startDate.AddMonths(1).AddDays(-1); // End of the target month

            return await _context.Bookings
                .Where(u => u.ServiceId == serviceId && u.StartDate >= startDate && u.StartDate <= endDate)
                .Include(b => b.Service)
                .Include(b => b.AddedBy)
                .OrderByDescending(b => b.CreatedAt)
                .Select(u => u.StartDate)
                .ToListAsync();
        }

        public async Task<PagedResult<Booking>> GetAllByServiceAsync(int curPage, int serviceId, BookingStatus status)
        {
            if (status == BookingStatus.All)
            {
                return await _context.Bookings
                .Where(b => b.ServiceId == serviceId)
                .Include(b => b.Service)
                .Include(b => b.AddedBy)
                .OrderByDescending(b => b.CreatedAt)
                .GetPaged(curPage);
            }
            else
            {
                return await _context.Bookings
                .Where(b => b.ServiceId == serviceId && b.Status == status)
                .Include(b => b.Service)
                .Include(b => b.AddedBy)
                .OrderByDescending(b => b.CreatedAt)
                .GetPaged(curPage);
            }
        }
        public async Task<PagedResult<Booking>> GetAllByProviderAsync(int curPage, string serviceProviderId, BookingStatus status, int? serviceId = 0)
        {
            if (status == BookingStatus.All)
            {
                if (serviceId == 0)
                {
                    return await _context.Bookings
                      .Include(b => b.Service)
                      .Include(b => b.AddedBy)
                      .Where(b => b.Service.ServiceProviderId == serviceProviderId)
                      .OrderByDescending(b => b.CreatedAt)
                      .GetPaged(curPage);
                } else
                {
                    return await _context.Bookings
                        .Include(b => b.Service)
                        .Include(b => b.AddedBy)
                        .Where(b => b.Service.ServiceProviderId == serviceProviderId && b.ServiceId == serviceId)
                        .OrderByDescending(b => b.CreatedAt)
                        .GetPaged(curPage);
                }
            }
            else
            {
                if (serviceId == 0)
                {
                    return await _context.Bookings
                      .Include(b => b.Service)
                      .Include(b => b.AddedBy)
                      .Where(b => b.Service.ServiceProviderId == serviceProviderId && b.Status == status)
                      .OrderByDescending(b => b.CreatedAt)
                      .GetPaged(curPage);
                }
                else
                {
                    return await _context.Bookings
                      .Include(b => b.Service)
                      .Include(b => b.AddedBy)
                      .Where(b => b.Service.ServiceProviderId == serviceProviderId && b.Status == status && b.ServiceId == serviceId)
                      .OrderByDescending(b => b.CreatedAt)
                      .GetPaged(curPage);
                }
            }
        }

        public async Task<PagedResult<Booking>> GetAllByUserAsync(int curPage, string userId, BookingStatus status, int? serviceId = 0)
        {
            if (status == BookingStatus.All)
            {
                if (serviceId == 0)
                {
                    return await _context.Bookings
               .Where(b => b.AddedById == userId)
               .Include(b => b.Service)
               .Include(b => b.AddedBy)
               .OrderByDescending(b => b.CreatedAt)
               .GetPaged(curPage);
                } else
                {
                    return await _context.Bookings
               .Where(b => b.AddedById == userId && b.ServiceId == serviceId)
               .Include(b => b.Service)
               .Include(b => b.AddedBy)
               .OrderByDescending(b => b.CreatedAt)
               .GetPaged(curPage);
                }
                    
            }
            else
            {
                if (serviceId == 0)
                {
                    return await _context.Bookings
               .Where(b => b.AddedById == userId && b.Status == status)
               .Include(b => b.Service)
               .Include(b => b.AddedBy)
               .OrderByDescending(b => b.CreatedAt)
               .GetPaged(curPage);
                }
                else
                {
                    return await _context.Bookings
               .Where(b => b.AddedById == userId && b.Status == status && b.ServiceId == serviceId)
               .Include(b => b.Service)
               .Include(b => b.AddedBy)
               .OrderByDescending(b => b.CreatedAt)
               .GetPaged(curPage);
                }
            }
        }

        public async Task<Booking?> GetEntityAsync(int? id)
        {
            return await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.AddedBy)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task DeleteEntityAsync(int? id)
        {
            var entity = await _context.Bookings.FindAsync(id);

            if (entity != null)
            {
                //Delete all comms for this booking
                var comms = await _context.MessageThreads.Where(c => c.ResourceId == entity.Id && c.ResourceType == MessageResourceType.Booking).ToListAsync();
                _context.MessageThreads.RemoveRange(comms);

                //Delete entity
                _context.Bookings.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateEntityAsync(Booking entity)
        {
            try
            {
                _context.Bookings.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> CreateEntityAsync(Booking booking)
        {
            booking.Status = BookingStatus.Open;
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return booking.Id;
        }

    }
}
