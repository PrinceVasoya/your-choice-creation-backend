# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["EcommerceCA/EcommerceCA.API/EcommerceCA.API.csproj", "EcommerceCA/EcommerceCA.API/"]
COPY ["EcommerceCA/EcommerceCA.Application/EcommerceCA.Application.csproj", "EcommerceCA/EcommerceCA.Application/"]
COPY ["EcommerceCA/EcommerceCA.Infrastructure/EcommerceCA.Infrastructure.csproj", "EcommerceCA/EcommerceCA.Infrastructure/"]
COPY ["EcommerceCA/EcommerceCA.Common/EcommerceCA.Common.csproj", "EcommerceCA/EcommerceCA.Common/"]
COPY ["EcommerceCA/EcommerceCA.Domain/EcommerceCA.Domain.csproj", "EcommerceCA/EcommerceCA.Domain/"]

RUN dotnet restore "EcommerceCA/EcommerceCA.API/EcommerceCA.API.csproj"

# Copy the remaining files and build the app
COPY . .
WORKDIR "/src/EcommerceCA/EcommerceCA.API"
RUN dotnet publish "EcommerceCA.API.csproj" -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port and configure ASP.NET Core URL
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "EcommerceCA.API.dll"]
