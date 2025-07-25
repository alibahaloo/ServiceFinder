# syntax=docker/dockerfile:1

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

# Set the working directory inside the container
WORKDIR /source

# Copy both Kimia.API and Kimia.Core projects into the container
COPY ./ServiceFinder ./ServiceFinder

# Move into Kimia.API directory for build
WORKDIR /source/ServiceFinder

ARG TARGETARCH

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -a ${TARGETARCH/amd64/x64} --use-current-runtime --self-contained false -o /app

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

# Install globalization support and ca-certificates for SSL/TLS
RUN apk add --no-cache icu-libs ca-certificates

# Set the DOTNET_SYSTEM_GLOBALIZATION_INVARIANT environment variable to false
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Update the CA certificates
RUN update-ca-certificates

COPY --from=build /app .

# Copy the SSL certificate from the host to the container
COPY Kimia/aspnetapp.pfx /https/aspnetapp.pfx

# Set environment variables for Kestrel to use HTTPS
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=your_password

EXPOSE 443
EXPOSE 80

USER $APP_UID

ENTRYPOINT ["dotnet", "ServiceFinder.dll"]
