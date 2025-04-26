FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ChessFiguresClassification_Api/ChessFiguresClassification_Api.csproj", "ChessFiguresClassification_Api/"]
RUN dotnet restore "ChessFiguresClassification_Api/ChessFiguresClassification_Api.csproj"
COPY . .
WORKDIR "/src/ChessFiguresClassification_Api"
RUN dotnet build "ChessFiguresClassification_Api.csproj" -c Release -o /app/build

# ✅ THIS is the missing stage
FROM build AS publish
WORKDIR "/src/ChessFiguresClassification_Api"
RUN dotnet publish "ChessFiguresClassification_Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChessFiguresClassification_Api.dll"]
