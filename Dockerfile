# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + projects
COPY *.sln ./

COPY InnSales.Api/*.csproj ./InnSales.Api/
COPY InnSales.Common/*.csproj ./InnSales.Common/
COPY InnSales.DataBase/*.csproj ./InnSales.DataBase/
COPY InnSales.Domain/*.csproj ./InnSales.Domain/
COPY InnSales.Services/*.csproj ./InnSales.Services/

# Restore
RUN dotnet restore InnSales.Api/InnSales.Api.csproj

# Copy full source
COPY . .

# Publish API
RUN dotnet publish InnSales.Api/InnSales.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "InnSales.Api.dll"]