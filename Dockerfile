FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app
 
# Copy solution and project files with correct casing
COPY *.sln ./
COPY MyApp/*.csproj ./MyApp/
COPY Pps.Api/*.csproj ./Pps.Api/
COPY pps.DataBase/*.csproj ./pps.DataBase/
COPY pps.Domain/*.csproj ./pps.Domain/
COPY pps.DTO/*.csproj ./pps.DTO/
COPY Pps.Services/*.csproj ./Pps.Services/
COPY pps.Tests/*.csproj ./pps.Tests/
 
# Restore dependencies
RUN dotnet restore
 
# Copy everything else
COPY . ./
 
#  Debug: List contents of /app before publishing
RUN echo "Listing contents of /app:" && ls -R /app
 
# Publish the Web API project
RUN dotnet publish Pps.Api/Pps.Api.csproj -c Release -o /app/out
 
# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .
 
EXPOSE 80
# Start the Web API
ENTRYPOINT ["dotnet", "Pps.Api.dll"]