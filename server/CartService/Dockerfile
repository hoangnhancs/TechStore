FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY server/BuildingBlocks/Shared.Core.EF/Shared.Core.EF.csproj server/BuildingBlocks/Shared.Core.EF/
COPY server/BuildingBlocks/Infrastructure.EF/Infrastructure.EF.csproj server/BuildingBlocks/Infrastructure.EF/
COPY server/BuildingBlocks/Shared.Web/Shared.Web.csproj server/BuildingBlocks/Shared.Web/
COPY server/CartService/CartService.csproj server/CartService/

RUN dotnet restore server/CartService/CartService.csproj

COPY server/BuildingBlocks/Shared.Core.EF/ server/BuildingBlocks/Shared.Core.EF/
COPY server/BuildingBlocks/Infrastructure.EF/ server/BuildingBlocks/Infrastructure.EF/
COPY server/BuildingBlocks/Shared.Web/ server/BuildingBlocks/Shared.Web/
COPY server/CartService/ server/CartService/

RUN dotnet publish server/CartService/CartService.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "CartService.dll"]
