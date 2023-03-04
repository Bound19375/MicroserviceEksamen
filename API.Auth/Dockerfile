#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:latest AS win
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/aspnet:7.0.3-bullseye-slim-arm64v8 AS arm
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /src
COPY ["API.Auth/API.Auth.csproj", "API.Auth/"]
COPY ["Auth.Application/Auth.Application.csproj", "Auth.Application/"]
COPY ["Crosscut/Crosscut.csproj", "Crosscut/"]
COPY ["Auth.Database/Auth.Database.csproj", "Auth.Database/"]
COPY ["Auth.Infrastructure/Auth.Infrastructure.csproj", "Auth.Infrastructure/"]
COPY ["Identity.Database/Identity.Database.csproj", "Identity.Database/"]
RUN dotnet restore "API.Auth/API.Auth.csproj"
COPY . .
WORKDIR "/src/API.Auth"
RUN dotnet build "API.Auth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API.Auth.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM win AS finalWin
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.Auth.dll"]

FROM arm AS finalArm
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.Auth.dll"]