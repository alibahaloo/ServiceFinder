# ServiceFinder

ServiceFinder is a .NET 9 web platform for service discovery, enabling small businesses, freelancers, and local providers to showcase and offer their services to customers. It is built using ASP.NET Core Razor Pages and uses SQL Server as its database. The platform integrates with AWS for features such as file storage (S3) and email notifications, and it leverages ASP.NET Identity for user authentication and authorization.

---

## Architecture

ServiceFinder is built using **ASP.NET Core Razor Pages** with a **SQL Server** database backend. Razor Pages is a page-focused web framework that simplifies building dynamic, data-driven websites. It is designed to make coding page-centric scenarios easier and more productive.

### Razor Pages Highlights:

* Follows the MVVM (Model-View-ViewModel) pattern
* Clean separation of UI (cshtml) and server logic (PageModel classes)
* Built-in support for model binding, validation, and routing
* Ideal for scenarios where each page has its own controller-like logic

The platform uses Entity Framework Core for database interactions and integrates with AWS services for features like file storage and email notifications.

---

## Features

The platform is intended for service discovery, enabling service providers—such as small businesses, local vendors, and freelancers—to post their offerings and make them easily discoverable. Customers can search for services, connect with providers, and make bookings directly through the platform.

### Service Listings

The system supports a wide range of service providers, each with detailed, customizable profiles that include descriptions and images. Listings can be added and managed by the providers themselves.

### Easy Bookings

Booking functionality is integrated to streamline scheduling. Customers can view provider availability and submit booking requests. Providers are able to manage, approve, or decline these requests through their own dashboards.

### Direct Communication

A built-in messaging feature facilitates direct communication between customers and service providers. This ensures smooth coordination and enables clarification of service details.

### Customer Reviews

The platform supports customer feedback in the form of reviews, allowing users to share their experiences and inform others' decisions. This helps foster a trusted and transparent community.

### Full User Management

The platform provides separate, role-based dashboards and management tools for customers and service providers, offering personalized user experiences.

### Email Integration

Notification emails can be sent through third-party platforms (such as AWS SES) to keep users informed about account activity, booking updates, and more.
Can be integrated with third-party platforms (such as AWS) to send email notifications.

### Business Registration

Supports multiple provider types including freelancers and registered businesses. Registration requests are reviewed before approval.

### Approval System

Includes an admin-level review and approval workflow to help prevent spam and ensure only valid service providers go live on the platform.

### Pricing Tiers

Fully customizable pricing plans allow configuration of features, pricing, number of service postings, and more.

### Support Tickets

Includes a built-in support ticketing system where users and service providers can raise issues or questions. Platform admins can respond, track, and resolve tickets via a centralized interface.
Notification emails can be sent through third-party platforms (such as AWS SES) to keep users informed about account activity, booking updates, and more.
Can be integrated with third-party platforms (such as AWS) to send email notifications.

---

## Prerequisites

Ensure you have the following installed on your development machine:

* [.NET SDK 9.0+](https://dotnet.microsoft.com/en-us/download)
* [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with C# extension
* [SQL Server](https://www.microsoft.com/en-ca/sql-server) installed and running (either as a direct instance or via Docker container)
* [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html) configured (for AWS integrations)
* [Docker](https://www.docker.com/) (required if using the containerized database setup)

---

## Setup Instructions

1. **Clone the repo**:

   ```bash
   git clone https://github.com/your-username/ServiceFinder.git
   cd ServiceFinder
   ```

2. **Restore dependencies**:

   ```bash
   dotnet restore
   ```

3. **Create migrations**:

   ```bash
   dotnet ef migrations add InitialCreate
   ```

4. **Apply EF Core migrations to the database**:

   ```bash
   docker-compose up -d

   # Ensure Docker DB container is fully running

   dotnet ef database update
   ```

5. **Add `appsettings.json`**:
   Create a file named `appsettings.json` in the root of your project with the following content:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=ServiceFinder;User=sa;Password=YourStrong!Passw0rd;MultipleActiveResultSets=true;TrustServerCertificate=True"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "EmailSettings": {
       "SmtpServer": "email-smtp.ca-central-1.amazonaws.com",
       "SmtpPort": 587,
       "SmtpUsername": "xxx",
       "SmtpPassword": "xxx",
       "SmtpFromEmail": "xxx"
     },
     "AllowedHosts": "*",
     "AWS": {
       "AccessKey": "xxx",
       "SecretKey": "xxx",
       "Region": "xxx"
     }
   }
   ```

6. **Add `InitResources.resx` under the `Utility` folder**:
   Create a resource file named `InitResources.resx` in the `Utility` directory. This file must include the following fields with values:

   * `AdminAddress`
   * `AdminEmail`
   * `AdminFullName`
   * `AdminPassword`
   * `AdminPhoneNumber`
   * `AdminPostalCode`
   * `City`
   * `Industry`
   * `Province`
   * `ServiceCategory`
   * `Subscriptions`

7. **Run the application**:

   ```bash
   dotnet run
   ```

---

## Docker Support

Docker is used to host the database container (SQL Server) for local development. A `docker-compose.yml` file is included for this purpose.

### Steps to use Docker:

1. Make sure Docker Desktop is running.

2. From the root directory of the project, run:

   ```bash
   docker-compose up -d
   ```

3. **Important**: The first time you run the application using Docker, it may fail because the database is not initialized.

4. Ensure that your `appsettings.json` file is configured to point to the Docker database container's connection string.

5. Then run the following commands locally to set up the schema and seed data:

   ```bash
   dotnet ef database update
   # Manually ensure InitResources.resx is created and populated
   dotnet run
   ```

6. From the second run onward, the application should run correctly with the Docker container.

---

## Dependencies & Credits

This project uses the following open-source packages:

* [`AWSSDK.Extensions.NETCore.Setup`](https://www.nuget.org/packages/AWSSDK.Extensions.NETCore.Setup) - AWS SDK support for dependency injection
* [`AWSSDK.S3`](https://www.nuget.org/packages/AWSSDK.S3) - Amazon S3 service client
* [`Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore`](https://www.nuget.org/packages/Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore) - Exception filters for EF Core
* [`Microsoft.AspNetCore.Identity.EntityFrameworkCore`](https://www.nuget.org/packages/Microsoft.AspNetCore.Identity.EntityFrameworkCore) - ASP.NET Identity integration with EF Core
* [`Microsoft.AspNetCore.Identity.UI`](https://www.nuget.org/packages/Microsoft.AspNetCore.Identity.UI) - Identity UI components
* [`Microsoft.EntityFrameworkCore.SqlServer`](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer) - SQL Server support for EF Core
* [`Microsoft.EntityFrameworkCore.Tools`](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Tools) - EF Core tools for design-time operations
* [`Microsoft.VisualStudio.Web.CodeGeneration.Design`](https://www.nuget.org/packages/Microsoft.VisualStudio.Web.CodeGeneration.Design) - Scaffolding tools
* [`Vereyon.Web.FlashMessage`](https://www.nuget.org/packages/Vereyon.Web.FlashMessage) - Flash messaging system for ASP.NET

---

## Project Structure

```
ServiceFinder/
├── .github/                  # GitHub workflows and settings
├── API/                      # API layer and endpoints
├── Areas/                    # MVC areas for modular features
├── bin/                      # Build output directory
├── Data/                     # EF Core data context and seed logic
├── EmailTemplates/           # Predefined HTML templates for emails
├── Migrations/               # EF Core migration files
├── obj/                      # Temporary build files
├── Pages/                    # Razor Pages UI logic
├── Properties/               # Project configuration files
├── Repos/                    # Repository pattern implementations
├── Utility/                  # Utility classes and InitResources.resx
├── wwwroot/                  # Static web assets (JS, CSS, images)
├── .gitattributes            # Git settings for text/binary handling
├── .gitignore                # Ignored files and folders
├── appsettings.*.json        # Configuration files per environment
├── compose.yaml              # Docker Compose file for DB container
├── Dockerfile                # Docker build instructions
├── libman.json               # Library Manager config (for client libraries)
├── LICENSE.txt               # Apache 2.0 License
├── Program.cs                # Application entry point
├── README.md                 # Project overview and setup instructions
├── ServiceFinder.csproj      # Project file
├── ServiceFinder.csproj.user # User-specific project config
└── ServiceFinder.sln         # Visual Studio solution file
```

---

## License

This project, **ServiceFinder**, is licensed under the Apache License 2.0.

You are free to use, modify, and distribute this software, provided that you:

* Include proper attribution to the original author.
* Retain the license and notice files in any redistribution.

See the `NOTICE` file for attribution details.

---

© 2025 Ali Bahaloo
