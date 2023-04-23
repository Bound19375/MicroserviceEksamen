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
COPY ["BoundBot/BoundBot.csproj", "BoundBot/"]
COPY ["BoundBot.Connection/BoundBot.Connection.csproj", "BoundBot.Connection/"]
COPY ["Crosscutting/Crosscutting.csproj", "Crosscutting/"]
RUN dotnet restore "BoundBot/BoundBot.csproj"
COPY . .
WORKDIR "/src/BoundBot"
RUN dotnet build "BoundBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BoundBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM win AS finalWin
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoundBot.dll"]

FROM arm AS finalArm
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoundBot.dll"]