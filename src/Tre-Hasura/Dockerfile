#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Tre-Hasura/Tre-Hasura.csproj", "src/Tre-Hasura/"]
RUN dotnet restore "src/Tre-Hasura/Tre-Hasura.csproj"
COPY . .
WORKDIR "/src/src/Tre-Hasura"
RUN dotnet build "Tre-Hasura.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tre-Hasura.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tre-Hasura.dll"]