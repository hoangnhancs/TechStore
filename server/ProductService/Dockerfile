FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY TechStore.sln ./
COPY server/ProductService/ProductService.csproj ./server/ProductService/
COPY server/Contract/Contract.csproj ./server/Contract/
COPY server/BuildingBlocks/Shared.Core.EF/Shared.Core.EF.csproj ./server/BuildingBlocks/Shared.Core.EF/
COPY server/BuildingBlocks/Infrastructure.EF/Infrastructure.EF.csproj ./server/BuildingBlocks/Infrastructure.EF/
COPY server/BuildingBlocks/Shared.Web/Shared.Web.csproj ./server/BuildingBlocks/Shared.Web/

RUN dotnet restore "./server/ProductService/ProductService.csproj"

# Copy all source code
COPY . .

# Publish
RUN dotnet publish "./server/ProductService/ProductService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProductService.dll"]