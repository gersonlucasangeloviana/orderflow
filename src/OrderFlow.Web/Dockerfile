FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.20 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/OrderFlow.Web/OrderFlow.Web.csproj -c Release -o /app/publish
FROM nginx:1.27.3-alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
