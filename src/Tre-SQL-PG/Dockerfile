#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Tre-SQL-PG/Tre-SQL-PG.csproj", "Tre-SQL-PG/"]
RUN dotnet restore "Tre-SQL-PG/Tre-SQL-PG.csproj"
COPY . .
WORKDIR "/src/Tre-SQL-PG"
RUN dotnet build "Tre-SQL-PG.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tre-SQL-PG.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV COMPlus_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "Tre-SQL-PG.dll"]