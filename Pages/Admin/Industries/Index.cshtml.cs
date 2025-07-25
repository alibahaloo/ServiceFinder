using ServiceFinder.Data;
using ServiceFinder.Repos;

namespace ServiceFinder.Pages.Admin.Industries
{
    public class IndexModel : AdminPageModel
    {
        private readonly IndustryRepo _repo;
        public int CurPage { get; set; }
        public int PageCount { get; set; }
        public IndexModel(IndustryRepo repo)
        {
            _repo = repo;
        }

        public IList<Industry> Industries { get; set; } = default!;

        public async Task OnGetAsync(int curPage = 1)
        {
            CurPage = curPage;

            var result = await _repo.GetAllAsync(CurPage);
            Industries = result.Data;

            PageCount = result.PageCount;
        }
    }
}
