using System.ComponentModel.DataAnnotations;

namespace ServiceFinder.Data.InputModels
{
    public class Message
    {
        public bool IsEnabled { get; set; } = true;
        public int? ThreadId { get; set; } = default!;
        public int ResourceId { get; set; } = default!;
        public MessageResourceType ResourceType { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string Body { get; set; } = string.Empty;
    }

    public class BackPartialModel
    {
        public string Text { get; set; } = default!;
        public string Url { get; set; } = default!;
    }

    public class CalendarPartialModel
    {
        public string Month { get; set; } = default!;
        public int Year { get; set; } = default!;
        public List<DateOnly> Dates { get; set; } = default!;
    }

    public class UnavailableDate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Regarding Service")]
        public int ServiceId { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateOnly Date { get; set; } = default!;
    }

    public class SupportTicket
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = default!;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; } = default!;
        public bool IsAccessRequest { get; set; } = false;

    }

    public class ApplicationUser
    {
        public string Id { get; set; } = default!;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = default!;

        [Required]
        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; } = default!;

        [Display(Name = "Address Line 2")]
        public string? AddressLine2 { get; set; }

        [Required]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = default!;

        [Required]
        [Display(Name = "City")]
        public int CityId { get; set; }

        [Required]
        [Display(Name = "Subscription")]
        public int SubscriptionId { get; set; }

        //Defines the user type
        [Required]
        [Display(Name = "User Type")]
        public string UserType { get; set; } = default!;

        //For Providers
        [Required]
        [Display(Name = "Verified")]
        public bool IsVerified { get; set; } = false;

        [Required]
        [Display(Name = "Phone Number")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 7)]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; } = default!;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        public ServiceProviderType ServiceProviderType { get; set; } = default!;
    }

    public class Invitation
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; } = default!;
        public int RetryCount { get; set; }
    }
    public class Review
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Review")]
        public string Body { get; set; } = string.Empty;
        public int Stars { get; set; }
    }
    public class Booking
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string Description { get; set; } = default!;

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; } = default!;

        [Required]
        [Display(Name = "Hours needed")]
        [Range(1, 100, ErrorMessage = "Number should be between 1 and 100")]

        public int NumberOfHours { get; set; } = 1;
    }
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Service Category")]
        public string Name { get; set; } = default!;

        [Display(Name = "Industry")]
        public int IndustryId { get; set; }
        [Display(Name = "Is A Navigation Item")]
        public bool IsNavItem { get; set; } = false;
    }

    public class City
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "City")]
        public string Name { get; set; } = default!;

        [Required]
        [Display(Name = "Province")]
        public int ProvinceId { get; set; }
        [Display(Name = "Is A Navigation Item")]
        public bool IsNavItem { get; set; } = false;
    }

    public class Service
    {
        public int Id { get; set; }
        [Display(Name = "Logo URL")]
        public string? LogoUrl { get; set; } = default!;
        [Required]
        [Display(Name = "Business Name")]
        public string Name { get; set; } = default!;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = default!;

        [Required]
        [Display(Name = "Years in Business")]
        [Range(1, 100, ErrorMessage = "Number should be between 1 and 100")]
        public int YearsInBusiness { get; set; }

        [Required]
        [Display(Name = "Service Provider")]
        public string ServiceProviderId { get; set; } = string.Empty;


        [Required]
        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; } = default!;

        [Display(Name = "Address Line 2")]
        public string? AddressLine2 { get; set; }

        [Required]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = default!;

        [Required]
        [Display(Name = "City")]
        public int CityId { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = default!;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = default!;

        [Url]
        [Display(Name = "Website")]
        public string? Website { get; set; } = default!;

        [Required]
        [Display(Name = "Hourly Rate (CAD)")]
        [Range(1, 1000, ErrorMessage = "Amount should be between 1.00 and 1000.00")]
        public decimal HourlyRate { get; set; } = default!;

        [Display(Name = "Cities")]
        public List<ServiceCity> ServiceCities { get; set; } = new();

        [Display(Name = "Categories")]
        public List<ServiceCategory> ServiceCategories { get; set; } = new();
    }
}
