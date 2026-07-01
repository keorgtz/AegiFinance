# ── 1. Restore (cache-friendly: solo los .csproj) ─────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src

COPY ["AegiFinance.sln", "."]
COPY ["src/AegiFinance.Domain/AegiFinance.Domain.csproj",           "src/AegiFinance.Domain/"]
COPY ["src/AegiFinance.Application/AegiFinance.Application.csproj", "src/AegiFinance.Application/"]
COPY ["src/AegiFinance.Infrastructure/AegiFinance.Infrastructure.csproj", "src/AegiFinance.Infrastructure/"]
COPY ["src/AegiFinance.Web/AegiFinance.Web.csproj",                 "src/AegiFinance.Web/"]
COPY ["src/AegiFinance.Worker/AegiFinance.Worker.csproj",           "src/AegiFinance.Worker/"]

RUN dotnet restore "AegiFinance.sln"

# ── 2. Build & Publish ─────────────────────────────────────────────────────────
FROM restore AS build
COPY . .
RUN dotnet publish "src/AegiFinance.Web/AegiFinance.Web.csproj" \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ── 3. Runtime ─────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# curl necesario para el healthcheck del compose
RUN apt-get update && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AegiFinance.Web.dll"]
