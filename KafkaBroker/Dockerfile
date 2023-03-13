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
COPY ["KafkaBroker/KafkaBroker.csproj", "KafkaBroker/"]
RUN dotnet restore "KafkaBroker/KafkaBroker.csproj"
COPY . .
WORKDIR "/src/KafkaBroker"
RUN dotnet build "KafkaBroker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KafkaBroker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM win AS finalWin
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KafkaBroker.dll"]

FROM arn AS finalArm
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KafkaBroker.dll"]