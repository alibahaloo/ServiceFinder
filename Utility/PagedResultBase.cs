using Microsoft.EntityFrameworkCore;

namespace ServiceFinder.Utility
{
    public class ListPages
    {
        public string Url { get; set; } = string.Empty;
        public int PageCount { get; set; }
    }
    public static class EFExtensions
    {
        public static async Task<PagedResult<T>> GetPaged<T>(this IQueryable<T> query,
                                         int page, int pageSize = 10) where T : class
        {
            //const int PAGESIZE = 10;
            int PAGESIZE = pageSize;
            var result = new PagedResult<T>();
            result.CurrentPage = page;
            result.PageSize = PAGESIZE;
            result.RowCount = query.Count();


            var pageCount = (double)result.RowCount / PAGESIZE;
            result.PageCount = (int)Math.Ceiling(pageCount);

            var skip = (page - 1) * PAGESIZE;

            result.Data = await query.Skip(skip).Take(PAGESIZE).ToListAsync();

            return result;
        }
    }

    public abstract class PagedResultBase
    {
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }

        public int FirstRowOnPage
        {

            get { return (CurrentPage - 1) * PageSize + 1; }
        }

        public int LastRowOnPage
        {
            get { return Math.Min(CurrentPage * PageSize, RowCount); }
        }
    }

    public class PagedResult<T> : PagedResultBase where T : class
    {
        public IList<T> Data { get; set; }

        public PagedResult()
        {
            Data = new List<T>();
        }
    }
}
