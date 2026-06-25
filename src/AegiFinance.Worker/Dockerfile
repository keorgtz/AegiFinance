FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["AegiFinance.sln", "."]
COPY ["src/AegiFinance.Domain/AegiFinance.Domain.csproj", "src/AegiFinance.Domain/"]
COPY ["src/AegiFinance.Application/AegiFinance.Application.csproj", "src/AegiFinance.Application/"]
COPY ["src/AegiFinance.Infrastructure/AegiFinance.Infrastructure.csproj", "src/AegiFinance.Infrastructure/"]
COPY ["src/AegiFinance.Web/AegiFinance.Web.csproj", "src/AegiFinance.Web/"]
COPY ["src/AegiFinance.Worker/AegiFinance.Worker.csproj", "src/AegiFinance.Worker/"]

RUN dotnet restore "AegiFinance.sln"

COPY . .
RUN dotnet publish "src/AegiFinance.Worker/AegiFinance.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AegiFinance.Worker.dll"]
