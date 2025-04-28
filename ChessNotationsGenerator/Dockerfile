# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=8.0


#vzit full .net framework...  
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

#same here... 
# -------- Runtime Stage --------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

#zkusit vlozit dodnet restore  
#run as root 
# Create appuser here in runtime image
#RUN adduser --disabled-password --gecos "" appuser

COPY --from=build /publish .

#RUN mkdir -p /app/wwwroot/uploads && chown -R appuser:appuser /app/wwwroot/uploads 

#USER appuser

EXPOSE 8080

ENTRYPOINT ["dotnet", "ChessNotationsGenerator.dll"]
