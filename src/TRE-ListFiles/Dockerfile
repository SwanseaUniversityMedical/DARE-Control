#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TRE-ListFiles/TRE-ListFiles.csproj", "TRE-ListFiles/"]
RUN dotnet restore "TRE-ListFiles/TRE-ListFiles.csproj"
COPY . .
WORKDIR "/src/TRE-ListFiles"
RUN dotnet build "TRE-ListFiles.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TRE-ListFiles.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TRE-ListFiles.dll"]