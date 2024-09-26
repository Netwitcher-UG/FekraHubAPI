FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apt-get update && \
    apt-get install -y \
    libgdiplus \
    libx11-dev \
    libxcb1 \
    libx11-xcb1 \
    libxrender1 \
    libxext6 \
    fontconfig \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FekraHubAPI.csproj", "./"]
RUN dotnet restore "./FekraHubAPI.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "FekraHubAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FekraHubAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY libs/linux/libwkhtmltox.so /app/libs/linux/libwkhtmltox.so

COPY Controllers/1.png /app/Controllers/1.png

ENTRYPOINT ["dotnet", "FekraHubAPI.dll"]