FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:8.0.416 AS build
WORKDIR /src
COPY ["Tre-Camunda/Tre-Camunda.csproj", "Tre-Camunda/"]
COPY ["BL/BL.csproj", "BL/"]
COPY ["TRE.Credentials/Tre_Credentials.csproj", "TRE.Credentials/"]
RUN dotnet restore "Tre-Camunda/Tre-Camunda.csproj"
COPY BL/ BL/
COPY TRE.Credentials/ TRE.Credentials/
COPY Tre-Camunda/ Tre-Camunda/
RUN dotnet publish "Tre-Camunda/Tre-Camunda.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --self-contained=false \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
USER root
RUN apt-get update && apt-get install -y --no-install-recommends wget curl \
    && wget -q https://dl.min.io/client/mc/release/linux-amd64/mc -O /usr/local/bin/mc \
    && chmod +x /usr/local/bin/mc \
    && apt-get purge -y wget \
    && apt-get autoremove -y \
    && rm -rf /var/lib/apt/lists/*
USER app
EXPOSE 8080
EXPOSE 8081
WORKDIR /app
COPY --chown=app:app --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tre-Camunda.dll"]
