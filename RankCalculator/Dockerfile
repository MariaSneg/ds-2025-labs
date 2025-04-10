# Базовый образ для запуска (ASP.NET Core, а не просто Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

# Этап сборки (используем SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RankCalculator.csproj", "./"]
RUN dotnet restore "./RankCalculator.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./RankCalculator.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этап публикации
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RankCalculator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

ENV ASPNETCORE_URLS=http://0.0.0.0:5003;http://0.0.0.0:5004

# Финальный образ (используем ASP.NET Core)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RankCalculator.dll"]