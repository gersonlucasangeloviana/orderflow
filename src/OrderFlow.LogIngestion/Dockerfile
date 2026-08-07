FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/OrderFlow.LogIngestion/OrderFlow.LogIngestion.csproj -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/runtime:8.0-bookworm-slim
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderFlow.LogIngestion.dll"]
