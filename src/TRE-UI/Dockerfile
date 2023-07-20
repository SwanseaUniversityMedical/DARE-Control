#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

ENV ASPNETCORE_ENVIRONMENT=Development

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TRE-UI/TRE-UI.csproj", "TRE-UI/"]
COPY ["BL/BL.csproj", "BL/"]
RUN dotnet restore "TRE-UI/TRE-UI.csproj"
COPY . .
WORKDIR "/src/TRE-UI"
RUN dotnet build "TRE-UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TRE-UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TRE-UI.dll"]