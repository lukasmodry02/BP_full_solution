# syntax=docker/dockerfile:1

# -------- Base Image: Ubuntu with dependencies --------
FROM ubuntu:22.04 AS base

ENV LC_ALL=C.UTF-8 LANG=C.UTF-8

# Install necessary packages
RUN apt-get update && apt-get install -y \
    sudo wget git cmake build-essential \
    protobuf-compiler ffmpeg \
    libgtk-3-dev libgstreamer1.0-dev libavcodec-dev libswscale-dev \
    libavformat-dev libv4l-dev ocl-icd-dev freeglut3-dev \
    libgeotiff-dev libusb-1.0-0-dev libdc1394-dev \
    ca-certificates

# Install .NET SDK
WORKDIR /tmp
RUN wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get update && apt-get install -y dotnet-sdk-8.0

# -------- Fetch EmguCV repo --------
WORKDIR /mnt/emgu_repo
RUN git clone https://github.com/emgucv/emgucv emgucv
WORKDIR /mnt/emgu_repo/emgucv
RUN git fetch origin 4.8.0 && git checkout 4.8.0
RUN git submodule update --init --recursive
WORKDIR /mnt/emgu_repo/emgucv/platforms/ubuntu/22.04
RUN ./apt_install_dependency
RUN ./cmake_configure

# -------- Build Stage --------
FROM base AS build

WORKDIR /src

# Copy csproj and restore
COPY ChessNotationsGenerator/ChessNotationsGenerator.csproj ChessNotationsGenerator/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore ChessNotationsGenerator/ChessNotationsGenerator.csproj

# Copy rest of the project
COPY ChessNotationsGenerator/ ChessNotationsGenerator/
WORKDIR /src/ChessNotationsGenerator

# Build and publish
RUN mkdir -p /publish && \
    dotnet publish ChessNotationsGenerator.csproj -c Release -o /publish /p:UseAppHost=false

# -------- Runtime Stage --------
FROM base AS final

# Set up working directory
WORKDIR /app

# Copy the published output
COPY --from=build /publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ChessNotationsGenerator.dll"]
