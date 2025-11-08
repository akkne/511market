FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ResaleTelegramBot/ResaleTelegramBot.csproj", "ResaleTelegramBot/"]
RUN dotnet restore "ResaleTelegramBot/ResaleTelegramBot.csproj"
COPY . .
WORKDIR "/src/ResaleTelegramBot"
RUN dotnet build "./ResaleTelegramBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ResaleTelegramBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY MidasWalletBot/appsettings*.json ./
ENTRYPOINT ["dotnet", "ResaleTelegramBot.dll"]
