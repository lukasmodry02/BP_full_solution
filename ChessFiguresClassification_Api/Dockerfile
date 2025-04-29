FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ChessFiguresClassification_API/ChessFiguresClassification_API.csproj", "ChessFiguresClassification_API/"]
RUN dotnet restore "ChessFiguresClassification_API/ChessFiguresClassification_API.csproj"
COPY . .
WORKDIR "/src/ChessFiguresClassification_API"
RUN dotnet build "ChessFiguresClassification_API.csproj" -c Release -o /app/build
RUN dotnet publish "ChessFiguresClassification_API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ChessFiguresClassification_API.dll"]
