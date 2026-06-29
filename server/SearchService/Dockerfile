FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY server/BuildingBlocks/Shared.Core.EF/Shared.Core.EF.csproj server/BuildingBlocks/Shared.Core.EF/
COPY server/BuildingBlocks/Shared.Web/Shared.Web.csproj server/BuildingBlocks/Shared.Web/
COPY server/Contract/Contract.csproj server/Contract/
COPY server/SearchService/SearchService.csproj server/SearchService/

RUN dotnet restore server/SearchService/SearchService.csproj

COPY server/BuildingBlocks/Shared.Core.EF/ server/BuildingBlocks/Shared.Core.EF/
COPY server/BuildingBlocks/Shared.Web/ server/BuildingBlocks/Shared.Web/
COPY server/Contract/ server/Contract/
COPY server/SearchService/ server/SearchService/

RUN dotnet publish server/SearchService/SearchService.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "SearchService.dll"]
