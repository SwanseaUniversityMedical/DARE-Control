#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging. 

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

ENV ASPNETCORE_ENVIRONMENT=Development

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DARE-API/DARE-API.csproj", "DARE-API/"]
COPY ["BL/BL.csproj", "BL/"]
RUN dotnet restore "DARE-API/DARE-API.csproj"
RUN dotnet restore "BL/BL.csproj"

COPY . .

WORKDIR "/src/DARE-API"
RUN dotnet build "DARE-API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DARE-API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DARE-API.dll"]

COPY rootca.pem /usr/local/share/ca-certificates/chi-ansible.crt

RUN chmod 644 /usr/local/share/ca-certificates/*.crt && \
	update-ca-certificates
