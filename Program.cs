using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using ServiceFinder.Utility;
using System.Globalization;
using Vereyon.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsVerifiedPolicy", policy =>
    {
        policy.Requirements.Add(new IsVerifiedRequirement());
    });
});

builder.Services.AddRazorPages();

//Cookie Consent
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential 
    // cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
});

//Startup Services
builder.Services.AddHostedService<InitializationService>();
builder.Services.AddScoped<InitializeRolesService, InitializeRolesService>();
//Auth policy services
builder.Services.AddTransient<IAuthorizationHandler, IsVerifiedRequirementHandler>();
//Repositories
builder.Services.AddScoped<UserRepo>();
builder.Services.AddScoped<CityRepo>();
builder.Services.AddScoped<ProvinceRepo>();
builder.Services.AddScoped<IndustryRepo>();
builder.Services.AddScoped<ServiceCategoryRepo>();
builder.Services.AddScoped<ServiceRepo>();
builder.Services.AddScoped<SubscriptionRepo>();
builder.Services.AddScoped<ReviewRepo>();
builder.Services.AddScoped<BookingRepo>();
builder.Services.AddScoped<InvitationRepo>();
builder.Services.AddScoped<SupportTicketRepo>();
builder.Services.AddScoped<UnavailableDateRepo>();
builder.Services.AddScoped<NotificationRepo>();

var awsAccessKey = builder.Configuration.GetSection("AWS:AccessKey");
var SecretKey = builder.Configuration.GetSection("AWS:SecretKey");
var awsOptions = builder.Configuration.GetAWSOptions("AWS");
awsOptions.Credentials = new BasicAWSCredentials(awsAccessKey.Value, SecretKey.Value);
builder.Services.AddDefaultAWSOptions(awsOptions);

builder.Services.AddAWSService<IAmazonS3>();
//Utilities
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<CalendarService>();
builder.Services.AddScoped<AnonymizerService>();
builder.Services.AddFlashMessage();
builder.Services.AddHealthChecks();

var app = builder.Build();

var culture = CultureInfo.CreateSpecificCulture("en-US");
var dateformat = new DateTimeFormatInfo
{
    ShortDatePattern = "yyyy-MM-dd",
    LongDatePattern = "yyyy-MM-dd hh:mm:ss tt"
};
culture.DateTimeFormat = dateformat;

var supportedCultures = new[]
{
    culture
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
