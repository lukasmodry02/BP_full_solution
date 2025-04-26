# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=8.0

# -------- Build Stage --------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Step 1: Copy .csproj and restore
COPY ChessNotationsGenerator/ChessNotationsGenerator.csproj ChessNotationsGenerator/

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore ChessNotationsGenerator/ChessNotationsGenerator.csproj

# Step 2: Copy the full project
COPY ChessNotationsGenerator/ ChessNotationsGenerator/
WORKDIR /src/ChessNotationsGenerator

# Step 3: Publish the PROJECT, not the SOLUTION!
RUN mkdir -p /publish && \
    echo "Publishing project..." && \
    dotnet publish ChessNotationsGenerator.csproj -c Release -o /publish /p:UseAppHost=false || (echo "Publish failed!" && exit 1)

# Debug: show published files
RUN echo "Listing /publish contents:" && ls -l /publish

# -------- Runtime Stage --------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app
COPY --from=build /publish .

USER appuser

EXPOSE 8080

ENTRYPOINT ["dotnet", "ChessNotationsGenerator.dll"]
