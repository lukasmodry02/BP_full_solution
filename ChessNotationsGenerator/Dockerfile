# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=8.0

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY --link ChessNotationsGenerator/ChessNotationsGenerator.csproj ChessNotationsGenerator/
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.cache/msbuild \
    dotnet restore ChessNotationsGenerator/ChessNotationsGenerator.csproj

# Copy the rest of the source code
COPY --link ChessNotationsGenerator/ ChessNotationsGenerator/
WORKDIR /src/ChessNotationsGenerator

# Build and publish
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.cache/msbuild \
    dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final

# Create a non-root user and set permissions
RUN useradd -m appuser
WORKDIR /app
COPY --from=build /app/publish .
USER appuser

EXPOSE 8080

ENTRYPOINT ["dotnet", "ChessNotationsGenerator.dll"]
