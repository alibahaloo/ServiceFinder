using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using ServiceFinder.Utility;
using System.Globalization;
using Vereyon.Web;

namespace ServiceFinder.Pages.Bookings
{
    public class CreateModel : BasePageModel
    {
        public DateOnly BookingDate { get; set; } = default!;
        [BindProperty]
        public string SelectedMonth { get; set; } = default!;
        [BindProperty]
        public int SelectedYear { get; set; } = default!;
        public List<DateOnly> UnavailableDates { get; set; } = default!;
        [BindProperty]
        public Data.InputModels.Booking Input { get; set; } = default!;

        public Data.Service Service { get; set; } = default!;

        private readonly ServiceRepo _serviceRepo;
        private readonly BookingRepo _bookingRepo;
        private readonly EmailService _emailService;
        private readonly UnavailableDateRepo _unavailableDateRepo;
        private readonly NotificationRepo _notificationRepo;

        public CreateModel(IFlashMessage flashMessage, ServiceRepo serviceRepo, BookingRepo bookingRepo, UserManager<Data.ApplicationUser> userManager, EmailService emailService, UnavailableDateRepo unavailableDateRepo, NotificationRepo notificationRepo)
        : base(userManager, flashMessage)
        {
            _serviceRepo = serviceRepo;
            _bookingRepo = bookingRepo;
            _emailService = emailService;
            _unavailableDateRepo = unavailableDateRepo;
            _notificationRepo = notificationRepo;
        }
        private enum AdjacentMode
        {
            Next, Previous
        }
        private static (string Month, int Year) GetAdjacentMonths(string currentMonth, int currentYear, AdjacentMode mode )
        {
            if (!DateTime.TryParseExact(currentMonth, "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime currentDate))
            {
                throw new ArgumentException("Invalid month name provided.");
            }
            DateTime current = new(currentYear, currentDate.Month, 1);
            var result = mode switch
            {
                AdjacentMode.Next => current.AddMonths(1),
                AdjacentMode.Previous => current.AddMonths(-1),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Invalid mode provided."),
            };
            return (result.ToString("MMMM"), result.Year);
        }
        public IActionResult OnPostNextMonth(int serviceId, string CurrentMonth, int CurrentYear)
        {
            //Pass the current selected month, find the next, navigate to it
            var (Month, Year) = GetAdjacentMonths(CurrentMonth, CurrentYear, AdjacentMode.Next);
            return RedirectToPage(new { serviceId = Input.ServiceId, SelectedMonth = Month, SelectedYear = Year });
        }
        public IActionResult OnPostPrevMonth(int serviceId, string CurrentMonth, int CurrentYear)
        {
            //Pass the current selected month, find the previous, navigate to it
            var (Month, Year) = GetAdjacentMonths(CurrentMonth, CurrentYear, AdjacentMode.Next);
            return RedirectToPage(new { serviceId = Input.ServiceId, SelectedMonth = Month, SelectedYear = Year });
        }
        public async Task<IActionResult> OnGet(int serviceId, string SelectedMonth, int SelectedYear, DateOnly BookingDate)
        {
            if (serviceId == 0)
            {
                return NotFound();
            }

            var service = await _serviceRepo.GetEntityAsync(serviceId);

            if (service == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            /*
            if (user == null)
            {
                return Unauthorized();
            }
            */
            //Check if the user is a service provider (an an owner)
            if (user?.UserType == UserAccountRoles.ServiceProvider)
            {
                _flashMessage.Danger("You cannot using this functionality when logged in as a 'Service Provider'. Log in as a 'Customer' to be able to do so.");
                return Redirect("/Services");
            }

            //Check if the user has already a booking with this service provider
            if (user != null)
            {
                var existingBooking = await _bookingRepo.GetExistingBookingAsync(serviceId, user.Id);
                if (existingBooking != null)
                {
                    _flashMessage.Warning("You already have a booking with this service. You can't make a new one while the existing one still open but you can edit it if you wish.");
                    return Redirect($"/Bookings/Details?Id={existingBooking.Id}");
                }
            }
            
            if (SelectedMonth != null)
            {
                this.SelectedMonth = SelectedMonth;
            }
            else
            {
                this.SelectedMonth = DateTime.Now.ToString("MMMM");
            }

            if (SelectedYear != 0)
            {
                this.SelectedYear = SelectedYear;
            }
            else
            {
                this.SelectedYear = DateTime.Now.Year;
            }

            if (BookingDate >= DateOnly.FromDateTime(DateTime.Today))
            {
                this.BookingDate = BookingDate;
            }
            else
            {
                this.BookingDate = DateOnly.FromDateTime(DateTime.Today);
            }

            //Get a list of unavailable dates for this service
            var unavailableDates = await _unavailableDateRepo.GetAllUnavailableDatesAsync(service.Id, this.SelectedMonth, this.SelectedYear);

            //get a list of other bookings from other customers
            var curBookedDates = await _bookingRepo.GetAllBookedDatesAsync(service.Id, this.SelectedMonth, this.SelectedYear);

            UnavailableDates = unavailableDates.Concat(curBookedDates).ToList();

            Service = service;

            Input = new() { ServiceId = serviceId };

            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostBookingAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var service = await _serviceRepo.GetEntityAsync(Input.ServiceId);

            if (service == null)
            {
                return NotFound();
            }

            if (Input.Description == null || Input.Description.Length == 0)
            {
                _flashMessage.Warning("Description cannot be empty!");
                return RedirectToPage(new { serviceId = Input.ServiceId, SelectedYear, BookingDate = Input.StartDate });
            }

            //check given date is today or in the future
            if (Input.StartDate < DateOnly.FromDateTime(DateTime.Now))
            {
                _flashMessage.Danger("Choose a date that is either today or in the future.");
                return RedirectToPage(new { serviceId = Input.ServiceId, SelectedYear, BookingDate = Input.StartDate });
            }

            //check if service provider is available
            if (await _unavailableDateRepo.CheckDateExistsAsync(Input.StartDate, Input.ServiceId))
            {
                _flashMessage.Danger("The selected date is not available for booking.");
                return RedirectToPage(new { serviceId = Input.ServiceId, SelectedMonth, SelectedYear, BookingDate = Input.StartDate });
            }

            //check if there's already a booking for that date
            if (await _bookingRepo.IsDateAlreadyBooked(Input.ServiceId, Input.StartDate))
            {
                _flashMessage.Danger("The selected date is already booked by another customer.");
                return RedirectToPage(new { serviceId = Input.ServiceId, SelectedYear, BookingDate = Input.StartDate });
            }

            Data.Booking booking = new()
            {
                ServiceId = Input.ServiceId,
                Description = Input.Description,
                StartDate = Input.StartDate,
                NumberOfHours = Input.NumberOfHours,
            };

            int newId = await _bookingRepo.CreateEntityAsync(booking);

            //Send an email to user confirming the booking was made
            if (user.Email != null && await _notificationRepo.GetUserNotificationStatusByFunction(NotificationFunction.BookingCreated, user.Id))
                _emailService.SendBookingConfirmation(user.Email, service.Name, service.ServiceProvider.FullName, $"/Bookings/Details?Id={newId}");

            //Send an email to the service provider confirming the receipt of the booking
            if (service.ServiceProvider.Email != null && await _notificationRepo.GetUserNotificationStatusByFunction(NotificationFunction.BookingCreated, service.ServiceProvider.Id))
                _emailService.SendBookingReceived(user.FullName, service.ServiceProvider.Email, service.Name, $"/Bookings/Details?Id={newId}");

            _flashMessage.Confirmation("Booking Saved Successfully! A confirmation email has been sent to you and the service provider. The service provider will review and reply accordingly. " +
                "Use the messaging system below to communicate with the service provider directly about this booking.");

            return RedirectToPage("/Bookings/Details", new { Id = newId });
        }
    }
}
