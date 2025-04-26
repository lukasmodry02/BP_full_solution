# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=8.0

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src
# Copy csproj and restore as distinct layers
COPY --link ChessFiguresClassification_Api/ChessFiguresClassification_Api.csproj ChessFiguresClassification_Api/
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.cache/msbuild \
    dotnet restore "ChessFiguresClassification_Api/ChessFiguresClassification_Api.csproj"
# Copy everything else and build
COPY --link ChessFiguresClassification_Api/. ChessFiguresClassification_Api/
WORKDIR /src/ChessFiguresClassification_Api
RUN dotnet build "ChessFiguresClassification_Api.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR /src/ChessFiguresClassification_Api
RUN dotnet publish "ChessFiguresClassification_Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app
# Create a non-root user and switch to it
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
COPY --from=publish /app/publish .
USER appuser
ENTRYPOINT ["dotnet", "ChessFiguresClassification_Api.dll"]
