#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TREAgent/TREAgent.csproj", "TREAgent/"]
COPY ["BL/BL.csproj", "BL/"]
RUN dotnet restore "TREAgent/TREAgent.csproj"
RUN dotnet restore "BL/BL.csproj"

COPY . .
WORKDIR "/src/TREAgent"
RUN dotnet build "TREAgent.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TREAgent.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TREAgent.dll"]

COPY rootca.pem /usr/local/share/ca-certificates/chi-ansible.crt

RUN chmod 644 /usr/local/share/ca-certificates/*.crt && \
	update-ca-certificates