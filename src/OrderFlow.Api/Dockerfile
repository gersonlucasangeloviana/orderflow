FROM mcr.microsoft.com/dotnet/sdk:8.0.12-alpine3.20 AS build
WORKDIR /src
COPY . .
RUN dotnet publish OrderFlow.Api.csproj -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:8.0.12-alpine3.20
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "OrderFlow.Api.dll"]
