# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=8.0

# -------- Build Stage --------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Step 1: Copy csproj and restore NuGet packages
COPY --link ChessNotationsGenerator/ChessNotationsGenerator.csproj ChessNotationsGenerator/
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.cache/msbuild \
    echo "✅ Restoring NuGet packages..." && \
    dotnet restore ChessNotationsGenerator/ChessNotationsGenerator.csproj

# Step 2: Copy the full project
COPY --link ChessNotationsGenerator/ ChessNotationsGenerator/
WORKDIR /src/ChessNotationsGenerator

# Step 3: Build and Publish the project into /publish
RUN mkdir -p /publish && \
    echo "✅ Publishing project to /publish..." && \
    dotnet publish -c Release -o /publish /p:UseAppHost=false || (echo "❌ Publish failed!" && exit 1)

# Step 4: Debug - List contents of the publish folder
RUN echo "✅ Listing /publish contents:" && ls -l /publish

# -------- Runtime Stage --------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final

RUN useradd -m appuser
WORKDIR /app

# Step 5: Copy published output from build stage
COPY --from=build /publish .

USER appuser

EXPOSE 8080

# Step 6: Start the application
ENTRYPOINT ["dotnet", "ChessNotationsGenerator.dll"]
