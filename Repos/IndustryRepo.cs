using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Utility;

namespace ServiceFinder.Repos
{
    public class IndustryRepo
    {
        private readonly ApplicationDbContext _context;

        public IndustryRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task DeleteEntityAsync(int? id)
        {
            var industry = await _context.Industries.FindAsync(id);

            if (industry != null)
            {
                _context.Industries.Remove(industry);
                await _context.SaveChangesAsync();
            }
        }
        public IList<Industry> GetAll()
        {
            return _context.Industries.ToList();
        }
        public async Task<PagedResult<Industry>> GetAllAsync(int curPage)
        {
            return await _context.Industries
                .OrderBy(i => i.Id)
                .GetPaged(curPage);
        }

        public async Task<Industry?> GetEntityAsync(int? id)
        {
            return await _context.Industries.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task UpdateEntityAsync(Industry industry)
        {
            try
            {
                _context.Industries.Update(industry);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task CreateEntityAsync(Industry industry)
        {
            if (_context.Industries == null || industry == null) { return; }
            _context.Industries.Add(industry);
            await _context.SaveChangesAsync();
        }
    }
}
