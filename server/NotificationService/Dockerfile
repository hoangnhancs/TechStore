FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY server/BuildingBlocks/Shared.Core.EF/Shared.Core.EF.csproj server/BuildingBlocks/Shared.Core.EF/
COPY server/BuildingBlocks/Infrastructure.EF/Infrastructure.EF.csproj server/BuildingBlocks/Infrastructure.EF/
COPY server/BuildingBlocks/Shared.Web/Shared.Web.csproj server/BuildingBlocks/Shared.Web/
COPY server/Contract/Contract.csproj server/Contract/
COPY server/EmailService/EmailService.csproj server/EmailService/
COPY server/NotificationService/NotificationService.csproj server/NotificationService/

RUN dotnet restore server/NotificationService/NotificationService.csproj

COPY server/BuildingBlocks/Shared.Core.EF/ server/BuildingBlocks/Shared.Core.EF/
COPY server/BuildingBlocks/Infrastructure.EF/ server/BuildingBlocks/Infrastructure.EF/
COPY server/BuildingBlocks/Shared.Web/ server/BuildingBlocks/Shared.Web/
COPY server/Contract/ server/Contract/
COPY server/EmailService/ server/EmailService/
COPY server/NotificationService/ server/NotificationService/

RUN dotnet publish server/NotificationService/NotificationService.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "NotificationService.dll"]
