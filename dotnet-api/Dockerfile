# ── Build stage ──────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ── Runtime stage ─────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create storage directory for uploaded files
RUN mkdir -p /app/storage

COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "EduPlatform.Api.dll"]
