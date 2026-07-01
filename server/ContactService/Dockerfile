FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY server/EmailService/EmailService.csproj server/EmailService/
COPY server/ContactService/ContactService.csproj server/ContactService/
RUN dotnet restore server/ContactService/ContactService.csproj

COPY server/EmailService/ server/EmailService/
COPY server/ContactService/ server/ContactService/
RUN dotnet publish server/ContactService/ContactService.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContactService.dll"]
