FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.20 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/OrderFlow.Admin.Mvc/OrderFlow.Admin.Mvc.csproj -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.20
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderFlow.Admin.Mvc.dll"]
