using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using ServiceFinder.Utility;
using System.ComponentModel.DataAnnotations;
using Vereyon.Web;

namespace ServiceFinder.Pages.Services
{
    public class IndexModel : BasePageModel
    {
        private readonly ServiceRepo _serviceRepo;
        private readonly ServiceCategoryRepo _categoryRepo;
        private readonly CityRepo _cityRepo;

        public int CurPage { get; set; }
        public int PageCount { get; set; }
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public class InputModel
        {
            public string Search { get; set; } = string.Empty;
            [Display(Name = "City")]
            public int CityId { get; set; }
            [Display(Name = "Category")]
            public int ServiceCategoryId { get; set; }
            [Display(Name = "Licensed businesses only")]
            public ServiceProviderType ServiceProviderType { get; set; }
        }

        public IndexModel(ServiceRepo serviceRepo, ServiceCategoryRepo categoryRepo, CityRepo cityRepo, IFlashMessage flashMessage, UserManager<Data.ApplicationUser> userManager)
             : base(userManager, flashMessage)
        {
            _serviceRepo = serviceRepo;
            _categoryRepo = categoryRepo;
            _cityRepo = cityRepo;
        }

        public IList<Data.Service> Services { get; set; } = default!;

        public async Task OnGetAsync(InputModel input, int curPage = 1)
        {
            CurPage = curPage;
            PopulateLists();

            //Doing the search
            var result = new PagedResult<Data.Service>();
            var query = _serviceRepo.GetSearchQuery();

            if (input.Search != string.Empty && input.Search != null)
                query = query.Where(
                    s => s.Name.ToLower().Contains(input.Search.ToLower()) || s.ServiceCategories.Any(sc => sc.Category.Name.ToLower().Contains(input.Search.ToLower()))
                    );

            if (input.CityId != 0)
                query = query.Where(s => s.ServiceCities.Any(c => c.CityId == input.CityId));

            if (input.ServiceCategoryId != 0)
                query = query.Where(s => s.ServiceCategories.Any(c => c.CategoryId == input.ServiceCategoryId));

            if (input.ServiceProviderType != ServiceProviderType.All)
                query = query.Where(s => s.ServiceProvider.ServiceProviderType == input.ServiceProviderType);
            /*
            if (input.IsLicensed != false)
            {
                query = query.Where(s => s.IsLicensed == true);
            }*/

            result = await _serviceRepo.SearchQuery(CurPage, query);

            Input = input;

            Services = result.Data;
            PageCount = result.PageCount;

        }

        private void PopulateLists()
        {
            SelectListItem firstItem = new() { Value = "0", Text = "All" };

            List<SelectListItem> categoryList = new SelectList(_categoryRepo.GetAll(), "Id", "Name").ToList();
            categoryList.Insert(0, firstItem);

            ViewData["ServiceCategoryId"] = categoryList;

            List<SelectListItem> cityList = new SelectList(_cityRepo.GetAll(), "Id", "Name").ToList();
            cityList.Insert(0, firstItem);

            ViewData["CityId"] = cityList;
        }
    }
}
