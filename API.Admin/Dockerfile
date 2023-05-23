#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:latest AS win
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/aspnet:7.0.5-bullseye-slim-arm64v8 AS arm
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["API.Admin/API.Admin.csproj", "API.Admin/"]
COPY ["Admin.Application/Admin.Application.csproj", "Admin.Application/"]
COPY ["Auth.Database/Auth.Database.csproj", "Auth.Database/"]
COPY ["Crosscutting/Crosscutting.csproj", "Crosscutting/"]
COPY ["Admin.Infrastructure/Admin.Infrastructure.csproj", "Admin.Infrastructure/"]
RUN dotnet restore "API.Admin/API.Admin.csproj"
COPY . .
WORKDIR "/src/API.Admin"
RUN dotnet build "API.Admin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API.Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM win AS finalWin
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.Admin.dll"]

FROM arm AS finalArm
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.Admin.dll"]