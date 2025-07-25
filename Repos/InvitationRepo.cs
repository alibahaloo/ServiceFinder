using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;

namespace ServiceFinder.Repos
{
    public class InvitationRepo
    {
        private readonly ApplicationDbContext _context;

        public InvitationRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> FindByEmail(string emailAddress)
        {
            var invitation = await _context.Invitations.Where(i => i.EmailAddress == emailAddress).FirstOrDefaultAsync();

            if (invitation != null)
            {
                return invitation.Id;
            }
            else
            {
                return 0;
            }
        }

        public async Task<Data.Invitation?> GetEntityAsync(int? id)
        {
            return await _context.Invitations.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task RetryAsync(int id)
        {
            var invitation = await _context.Invitations.FindAsync(id);

            if (invitation != null)
            {
                invitation.RetryCount++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task CreateEntityAsync(Data.Invitation invitation)
        {
            if (_context.Invitations == null || invitation == null) { return; }
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEntityAsync(int? id)
        {
            var invitation = await _context.Invitations.FindAsync(id);

            if (invitation != null)
            {
                _context.Invitations.Remove(invitation);
                await _context.SaveChangesAsync();
            }
        }
    }
}
