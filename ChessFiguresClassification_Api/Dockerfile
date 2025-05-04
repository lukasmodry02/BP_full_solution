# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=8.0

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ChessFiguresClassification_Api/ChessFiguresClassification_Api.csproj ChessFiguresClassification_Api/
RUN dotnet restore "ChessFiguresClassification_Api/ChessFiguresClassification_Api.csproj"

# Copy everything else and build
COPY ChessFiguresClassification_Api/. ChessFiguresClassification_Api/
WORKDIR /src/ChessFiguresClassification_Api
RUN dotnet build "ChessFiguresClassification_Api.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR /src/ChessFiguresClassification_Api
RUN dotnet publish "ChessFiguresClassification_Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app

# Run as root (default in base image)
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChessFiguresClassification_Api.dll"]
