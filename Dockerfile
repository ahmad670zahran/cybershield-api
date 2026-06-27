# ── Build Stage ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CyberShield.API.csproj ./
RUN dotnet restore

COPY Controllers/ ./Controllers/
COPY Data/ ./Data/
COPY DTOs/ ./DTOs/
COPY Filters/ ./Filters/
COPY Migrations/ ./Migrations/
COPY Models/ ./Models/
COPY Properties/ ./Properties/
COPY Services/ ./Services/
COPY Program.cs ./
COPY appsettings.json ./
COPY appsettings.Production.json ./

RUN dotnet publish -c Release -o /app/publish

# ── Runtime Stage ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN mkdir -p /app/data
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CyberShield.API.dll"]