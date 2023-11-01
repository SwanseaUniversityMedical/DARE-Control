#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base


ENV ASPNETCORE_ENVIRONMENT=Development

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Tre-Hasura/Tre-Hasura.csproj", "Tre-Hasura/"]
COPY ["BL/BL.csproj", "BL/"]
RUN dotnet restore "Tre-Hasura/Tre-Hasura.csproj"
RUN dotnet restore "BL/BL.csproj"

COPY . .
WORKDIR "/src/Tre-Hasura"
RUN dotnet build "Tre-Hasura.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tre-Hasura.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tre-Hasura.dll"]