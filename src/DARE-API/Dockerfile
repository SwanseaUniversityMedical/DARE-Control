FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:8.0.416 AS build
WORKDIR /src
COPY ["DARE-API/DARE-API.csproj", "DARE-API/"]
COPY ["BL/BL.csproj", "BL/"]
RUN dotnet restore "DARE-API/DARE-API.csproj"
COPY BL/ BL/
COPY DARE-API/ DARE-API/
RUN dotnet publish "DARE-API/DARE-API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --self-contained=false \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
ENV ASPNETCORE_ENVIRONMENT=Development
USER root
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY rootca.pem /usr/local/share/ca-certificates/chi-ansible.crt
RUN chmod 644 /usr/local/share/ca-certificates/chi-ansible.crt && \
    update-ca-certificates
USER app
EXPOSE 8080
EXPOSE 443
WORKDIR /app
COPY --chown=app:app --from=build /app/publish .
ENTRYPOINT ["dotnet", "DARE-API.dll"]
