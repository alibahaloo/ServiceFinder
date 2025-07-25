using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace ServiceFinder.Data
{
    public enum SubscriptionFunctions
    {
        SendMessage,
        PostService,
        ViewServiceContactInfo,
        ViewProviderContactInfo,
        BookService,
        WriteReview
    }
    public static class UserAccountRoles
    {
        public static List<string> GetList()
        {
            return new List<string> { Customer, ServiceProvider, Admin };
        }

        public const string Customer = "Customer";
        public const string ServiceProvider = "Service Provider";
        public const string Admin = "Admin";
    }

    public abstract class BaseEntity
    {
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; }
        public string AddedById { get; set; } = string.Empty;
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>().Navigation(e => e.Bookings).AutoInclude();
            base.OnModelCreating(modelBuilder);
            // Configure cascade deletion behavior
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var currentUserIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            var userId = currentUserIdClaim?.Value ?? "";

            //UserId has to be available
            //if (userId == null) { throw new ArgumentNullException(nameof(userId)); }

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.AddedById = userId;
                        entry.Entity.CreatedAt = now;
                        break;

                    case EntityState.Modified:
                        // Avoid modifying AddedById and CreatedAt properties during updates
                        entry.Property(x => x.AddedById).IsModified = false;
                        entry.Property(x => x.CreatedAt).IsModified = false;
                        entry.Entity.UpdatedAt = now;
                        break;
                }

                // Trim string properties
                foreach (var property in entry.OriginalValues.Properties)
                {
                    if (property.ClrType == typeof(string))
                    {
                        var currentValue = entry.CurrentValues[property];
                        entry.CurrentValues[property] = currentValue?.ToString()?.Trim();
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<Industry> Industries { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<UnavailableDate> UnavailableDates { get; set; }

        public DbSet<MessageThread> MessageThreads { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ServiceImage> ServiceImages { get; set; }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NewMessageThreadAlert> NewMessageThreadAlerts { get; set; }
    }

    public enum BookingStatus
    {
        All,
        Open,
        Confirm,
        Decline
    }

    public enum SupportTicketStatus
    {
        All,
        Open,
        Resolved
    }
    public enum NotificationFunction
    {
        BookingCreated,
        BookingUpdated,
        MessageReceived,
        SupportTicketCreated,
        SupportTicketUpdated,
        ReviewsReceived
    }
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;
        public NotificationFunction Function { get; set; }
        public bool Status { get; set; } = true; //by default, all notifications are on
    }

    public class ServiceImage : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; } = default!;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = default!;

        [Display(Name = "Logo URL")]
        public string ImageUrl { get; set; } = default!;

        public int Order { get; set; }
    }

    public class SupportTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = default!;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; } = default!;

        public string UserId { get; set; } = default!;

        [Display(Name = "Filed By")]
        public ApplicationUser User { get; set; } = default!;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        public SupportTicketStatus Status { get; set; }
        [Display(Name = "Access Request")]
        public bool IsAccessRequest { get; set; } = false;
    }

    public class UnavailableDate : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Regarding Service")]
        public int ServiceId { get; set; }

        [Display(Name = "Service")]
        public Service Service { get; set; } = default!;

        [Display(Name = "Added By")]
        public ApplicationUser AddedBy { get; set; } = default!;

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateOnly Date { get; set; } = default!;
    }

    public class Invitation : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; } = default!;

        public int RetryCount { get; set; } = 1;
    }

    public class Booking : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = default!;

        [Required]
        [Display(Name = "Regarding Service")]
        public int ServiceId { get; set; }

        [Display(Name = "Service")]
        public Service Service { get; set; } = default!;

        [Display(Name = "Booked By")]
        public ApplicationUser AddedBy { get; set; } = default!;

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; } = default
            !;
        [Required]
        [Display(Name = "Hours needed")]
        [Range(1, 100, ErrorMessage = "Number should be between 1 and 100")]
        public int NumberOfHours { get; set; } = 1;

        public BookingStatus Status { get; set; }

        [NotMapped] // This attribute indicates that the property is not mapped to the database
        [Display(Name = "Is Expired")]
        public bool IsExpired => StartDate <= DateOnly.FromDateTime(DateTime.Today);
    }

    public class Subscription : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;
        [Required]
        [Display(Name = "View Service Contact Info")]
        public bool CanViewServiceContactInfo { get; set; } = false;
        [Required]
        [Display(Name = "View Provider Contact Info")]
        public bool CanViewProviderContactInfo { get; set; } = false;

        [Required]
        [Display(Name = "Book Service")]
        public bool CanBookService { get; set; } = false;

        [Required]
        [Display(Name = "Write Review")]
        public bool CanWriteReview { get; set; } = false;

        [Display(Name = "# Services")]
        public int? MaxServices { get; set; }

        [Display(Name = "# Message Threads")]
        public int? MaxMessages { get; set; }

        [Required]
        [Display(Name = "Fee (CAD)")]
        [Range(0, 1000, ErrorMessage = "Amount should be between 0 and 1000.00")]
        public decimal Fee { get; set; }

        [Required]
        [Display(Name = "Must Contact To Apply")]
        public bool MustContactToApply { get; set; } = false;
        [Range(1, int.MaxValue, ErrorMessage = "Order has to be minimum 1")]
        public int Order { get; set; } = default!;
    }

    public class Review : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Body")]
        public string Body { get; set; } = default!;

        [Display(Name = "Sender")]
        public ApplicationUser AddedBy { get; set; } = default!;

        [Required]
        [Display(Name = "Regarding Service")]
        public int ServiceId { get; set; }

        [Display(Name = "Regarding Service")]
        public Service Service { get; set; } = default!;

        [Required]
        [Display(Name = "Rating")]
        [Range(1, 5, ErrorMessage = "Number should be between 1 and 5")]
        public int Stars { get; set; }
        [NotMapped]
        [Display(Name = "Is By Verified Customer")]
        public bool IsByVerifiedCustomer
        {
            get
            {
                if (AddedBy == null || AddedBy.Bookings == null || ServiceId == 0)
                {
                    return false;
                }

                // Check if the user has a booking for the specific service
                return AddedBy.Bookings.Any(b => b.ServiceId == ServiceId);
            }
        }
    }

    public class ApplicationUser : IdentityUser
    {
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
        public City City { get; set; } = default!;
        [Required]
        [Display(Name = "Subscription")]
        public int SubscriptionId { get; set; }
        [Display(Name = "Subscription")]
        public Subscription Subscription { get; set; } = default!;

        //Defines the user type
        [Required]
        [Display(Name = "User Type")]
        public string UserType { get; set; } = default!;

        //For Providers
        [Required]
        [Display(Name = "Verified")]
        public bool IsVerified { get; set; } = false;
        public ICollection<Service>? Services { get; set; } = default!;

        public DateTime? DeletedOn { get; set; }

        public string? ProfileURL { get; set; } = default!;
        [Display(Name = "JoinedOn")]
        public DateTime JoinedOn { get; set; } = default!;
        public ServiceProviderType ServiceProviderType { get; set; } = ServiceProviderType.Freelancer;
        public ICollection<Booking>? Bookings { get; set; } = default!;
    }

    public enum ServiceProviderType
    {
        All,
        Freelancer,
        RegisteredTrader
    }

    public enum MessageResourceType
    {
        All,
        Service,
        Booking,
        SupportTicket
    }

    public class Message : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int MessageThreadId { get; set; } = default!;
        public MessageThread MessageThread { get; set; } = default!;
        [Display(Name = "Sender")]
        public ApplicationUser AddedBy { get; set; } = default!;
        [Required]
        [Display(Name = "Body")]
        public string Body { get; set; } = default!;
    }

    public class MessageThread : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int ResourceId { get; set; } = default!;

        public MessageResourceType ResourceType { get; set; }
        [Display(Name = "Sender")]
        public ApplicationUser AddedBy { get; set; } = default!;

        [Display(Name = "Archived On")]
        public DateTime? ArchivedOn { get; set; }

        public string? ArchiverId { get; set; }

        [Display(Name = "Archived By")]
        public ApplicationUser Archiver { get; set; } = default!;

        public List<Message> Messages { get; set; } = default!;
        [NotMapped]
        public bool IsNew { get; set; } = false;
    }

    public class NewMessageThreadAlert
    {
        [Key]
        public int Id { get; set; }
        public int MessageThreadId { get; set; }
        // Navigation property for the related MessageThread
        public MessageThread MessageThread { get; set; } = default!;
        public string ApplicationUserId { get; set; } = default!;
        public ApplicationUser ApplicationUser { get; set; } = default!;
    }

    public class Service : BaseEntity
    {
        [Key]
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

        [Display(Name = "Service Provider")]
        public ApplicationUser ServiceProvider { get; set; } = default!;

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
        public virtual City City { get; set; } = default!;

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

        [Display(Name = "Offering Cities")]
        public List<ServiceCity> ServiceCities { get; set; } = new();

        [Display(Name = "Categories")]
        public List<ServiceCategory> ServiceCategories { get; set; } = new();

        [Display(Name = "Reviews")]
        public List<Review> Reviews { get; set; } = default!;

        [Display(Name = "Images")]
        public List<ServiceImage> ServiceImages { get; set; } = default!;
    }

    public class ServiceCity
    {
        [Key]
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int CityId { get; set; }
        public Service Service { get; set; } = null!;
        public City City { get; set; } = null!;
    }

    public class ServiceCategory
    {
        [Key]
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int CategoryId { get; set; }
        public Service Service { get; set; } = null!;
        public Category Category { get; set; } = null!;

    }

    public class City : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "City")]
        public string Name { get; set; } = default!;

        [Required]
        [Display(Name = "Province")]
        public int ProvinceId { get; set; }
        public virtual Province Province { get; set; } = default!;
        [Display(Name = "Is A Navigation Item")]
        public bool IsNavItem { get; set; } = false;
    }

    public class Province : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Province")]
        public string Name { get; set; } = default!;
    }

    public class Industry : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Industry")]
        public string Name { get; set; } = default!;
    }

    public class Category : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Service Category")]
        public string Name { get; set; } = default!;

        [Required]
        [Display(Name = "Industry")]
        public int IndustryId { get; set; }

        public virtual Industry Industry { get; set; } = default!;
        [Display(Name = "Is A Navigation Item")]
        public bool IsNavItem { get; set; } = false;
    }
}