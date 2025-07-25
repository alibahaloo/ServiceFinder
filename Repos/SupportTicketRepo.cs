using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Utility;

namespace ServiceFinder.Repos
{
    public class SupportTicketRepo
    {
        private readonly ApplicationDbContext _context;

        public SupportTicketRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Data.SupportTicket>> GetAllSupportTicketsAsync(int curPage, SupportTicketStatus status, string search = "")
        {
            var query = _context.SupportTickets
            .Include(s => s.User)
            .Where(s => status == SupportTicketStatus.All || s.Status == status);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.EmailAddress.Contains(search) || (s.User != null && s.User.UserName != null && s.User.UserName.Contains(search)));
            }

            query = query.OrderByDescending(s => s.CreatedAt); // Ensure ordering

            return await query.GetPaged(curPage);


        }

        public async Task<PagedResult<Data.SupportTicket>> GetMySupportTicketsAsync(int curPage, string userId, SupportTicketStatus status)
        {
            if (status == SupportTicketStatus.All)
            {
                return await _context.SupportTickets
                .Where(s => userId == s.UserId)
                .Include(s => s.User)
                .OrderByDescending(s => s.CreatedAt)
                .GetPaged(curPage);
            }
            else
            {
                return await _context.SupportTickets
                .Where(s => userId == s.UserId)
                .Where(s => s.Status == status)
                .Include(s => s.User)
                .OrderByDescending(s => s.CreatedAt)
                .GetPaged(curPage);
            }


        }

        public async Task<bool> IsUserOwnerAsync(string userEmail, int supportTicketId)
        {
            var supportTicket = await _context.SupportTickets.FindAsync(supportTicketId);

            if (supportTicket == null) { return false; }

            return (supportTicket.EmailAddress == userEmail);
        }

        public async Task<int> CreateEntityAsync(Data.SupportTicket supportTicket)
        {
            if (_context.SupportTickets == null || supportTicket == null) { throw new InvalidDataException("supportTicket == null"); }
            _context.SupportTickets.Add(supportTicket);
            await _context.SaveChangesAsync();

            return supportTicket.Id;
        }

        public async Task<Data.SupportTicket?> GetSupportTicketByEmail(string emailAddress)
        {
            return await _context.SupportTickets.Where(s => s.EmailAddress == emailAddress && s.Status == SupportTicketStatus.Open).FirstOrDefaultAsync();
        }

        public async Task<Data.SupportTicket?> GetSupportTicketById(int? id)
        {
            return await _context.SupportTickets.Include(s => s.User).FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task ResolveSupportTicket(string emailAddress)
        {
            var supportTicket = await _context.SupportTickets.Where(s => s.EmailAddress == emailAddress && s.Status == SupportTicketStatus.Open).FirstOrDefaultAsync();

            if (supportTicket == null) { return; };

            supportTicket.Status = SupportTicketStatus.Resolved;
            _context.SupportTickets.Update(supportTicket);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEntityAsync(Data.SupportTicket entity)
        {
            try
            {
                _context.SupportTickets.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
