FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY server/BuildingBlocks/Shared.Core.EF/Shared.Core.EF.csproj server/BuildingBlocks/Shared.Core.EF/
COPY server/BuildingBlocks/Infrastructure.EF/Infrastructure.EF.csproj server/BuildingBlocks/Infrastructure.EF/
COPY server/BuildingBlocks/Shared.Web/Shared.Web.csproj server/BuildingBlocks/Shared.Web/
COPY server/Contract/Contract.csproj server/Contract/
COPY server/ReviewService/ReviewService.csproj server/ReviewService/

RUN dotnet restore server/ReviewService/ReviewService.csproj

COPY server/BuildingBlocks/Shared.Core.EF/ server/BuildingBlocks/Shared.Core.EF/
COPY server/BuildingBlocks/Infrastructure.EF/ server/BuildingBlocks/Infrastructure.EF/
COPY server/BuildingBlocks/Shared.Web/ server/BuildingBlocks/Shared.Web/
COPY server/Contract/ server/Contract/
COPY server/ReviewService/ server/ReviewService/

RUN dotnet publish server/ReviewService/ReviewService.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "ReviewService.dll"]
