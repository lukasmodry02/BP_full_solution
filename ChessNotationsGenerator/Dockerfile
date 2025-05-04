# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=8.0

# -------- Build Stage --------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY ChessNotationsGenerator/ChessNotationsGenerator.csproj ChessNotationsGenerator/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore ChessNotationsGenerator/ChessNotationsGenerator.csproj

COPY ChessNotationsGenerator/ ChessNotationsGenerator/
WORKDIR /src/ChessNotationsGenerator

RUN mkdir -p /publish && \
    dotnet publish ChessNotationsGenerator.csproj -c Release -o /publish

# -------- Runtime Stage --------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

# Install native dependencies
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    libsm6 \
    libxext6 \
    libxrender-dev \
    libgtk2.0-dev \
    libglib2.0-0 \
    libgeotiff-dev \
    libtiff-dev \
    libgdal-dev \
    libusb-1.0-0 \
    libxt6 \
    libopengl0 \
    libgl1 \
    && ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /publish .

COPY ChessNotationsGenerator/native-libs/linux-x64/libcvextern.so /app/runtimes/ubuntu.20.04-x64/native/

ENV LD_LIBRARY_PATH=/app/runtimes/ubuntu.20.04-x64/native:$LD_LIBRARY_PATH

EXPOSE 8080

ENTRYPOINT ["dotnet", "ChessNotationsGenerator.dll"]
