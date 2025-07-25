using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using System.ComponentModel.DataAnnotations;
using Vereyon.Web;

namespace ServiceFinder.Pages.Bookings
{
    [Authorize]
    public class MyBookingsModel : BasePageModel
    {
        public class InputModel
        {
            public string Search { get; set; } = string.Empty;
            public BookingStatus Status { get; set; }
            [Display(Name = "Service")]
            public int ServiceId { get; set; }
        }
        private readonly BookingRepo _bookingRepo;
        private readonly ServiceRepo _serviceRepo;
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public int CurPage { get; set; }
        public int PageCount { get; set; }
        public IList<Booking> Bookings { get; set; } = default!;

        public MyBookingsModel(UserManager<ApplicationUser> userManager, IFlashMessage flashMessage, BookingRepo bookingRepo, ServiceRepo serviceRepo)
            : base(userManager, flashMessage)
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
        }

        public async Task<IActionResult> OnGet(InputModel input, int curPage = 1)
        {
            //Check if the current user is service provider or customer
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            Utility.PagedResult<Booking>? result;
            IList<Service> serviceList;
            if (user.UserType == UserAccountRoles.ServiceProvider)
            {
                serviceList = await _serviceRepo.GetProviderServicesAsync(user.Id);
                //Find all bookings for the provider (all of its services)
                result = await _bookingRepo.GetAllByProviderAsync(curPage, user.Id, input.Status, input.ServiceId);
            }
            else
            {
                serviceList = await _serviceRepo.GetAllAsync();
                //Find all bookings made by the current user
                result = await _bookingRepo.GetAllByUserAsync(curPage, user.Id, input.Status, input.ServiceId);
            }

            SelectListItem firstItem = new() { Value = "0", Text = "All Services" };
            List<SelectListItem> serviceSelectList = new SelectList(serviceList, "Id", "Name").ToList();
            serviceSelectList.Insert(0, firstItem);
            ViewData["ServiceId"] = serviceSelectList;

            CurPage = curPage;
            Bookings = result.Data;
            PageCount = result.PageCount;
            return Page();
        }
    }
}
