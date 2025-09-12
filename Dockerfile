# Multi-stage Dockerfile for .NET 8 ASP.NET Core MVC app

# 1) Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy sln and project files first for better layer caching
COPY EforTakipUygulamasi.sln ./
COPY EforTakipUygulamasi/EforTakipUygulamasi.csproj EforTakipUygulamasi/

# Restore NuGet packages
RUN dotnet restore "EforTakipUygulamasi/EforTakipUygulamasi.csproj"

# Copy the rest of the source
COPY . ./

# Publish (Release)
RUN dotnet publish "EforTakipUygulamasi/EforTakipUygulamasi.csproj" -c Release -o /app/publish /p:UseAppHost=false


# 2) Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Non-root user (defined in aspnet image)
USER app

# Expose default ASP.NET Core port
EXPOSE 8080

# Configure ASP.NET Core to listen on 8080
ENV ASPNETCORE_URLS=http://+:8080

# Ensure required directories exist (uploads)
RUN mkdir -p /app/wwwroot/uploads

ENTRYPOINT ["dotnet", "EforTakipUygulamasi.dll"]


