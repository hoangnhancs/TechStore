FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY server/GatewayService/GatewayService.csproj server/GatewayService/
RUN dotnet restore server/GatewayService/GatewayService.csproj

COPY server/GatewayService/ server/GatewayService/
RUN dotnet publish server/GatewayService/GatewayService.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "GatewayService.dll"]
