FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.20 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/OrderFlow.Notifications/OrderFlow.Notifications.csproj -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine3.20
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderFlow.Notifications.dll"]
