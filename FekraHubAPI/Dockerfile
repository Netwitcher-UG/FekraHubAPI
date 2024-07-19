# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["FekraHubAPI.csproj", "./"]
RUN dotnet restore "./FekraHubAPI.csproj"

# Copy the rest of the application files
COPY . .

# Ensure the build and publish directories exist and are clean
RUN mkdir -p /app/build_output /app/publish_output /app/logs && rm -rf /app/build_output/* /app/publish_output/*

# Build the application with the DestinationFolder parameter
RUN dotnet msbuild "FekraHubAPI.csproj" /p:Configuration=$BUILD_CONFIGURATION /p:OutputPath=/app/build_output /p:UseAppHost=false

# Publish the application
RUN dotnet publish "FekraHubAPI.csproj" -c $BUILD_CONFIGURATION --no-restore --output /app/publish_output /p:UseAppHost=false

# Use the official ASP.NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy the SSL certificate
COPY certs/aspnetapp.pfx /app/certs/aspnetapp.pfx

# Copy the published application to the final image
COPY --from=build /app/publish_output .

ENTRYPOINT ["dotnet", "FekraHubAPI.dll"]