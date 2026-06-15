FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:8.0.416 AS build
WORKDIR /src
COPY ["TRE-API/TRE-API.csproj", "TRE-API/"]
COPY ["BL/BL.csproj", "BL/"]
COPY ["TRE.Credentials/Tre_Credentials.csproj", "TRE.Credentials/"]
RUN dotnet restore "TRE-API/TRE-API.csproj"
COPY BL/ BL/
COPY TRE.Credentials/ TRE.Credentials/
COPY TRE-API/ TRE-API/
RUN dotnet publish "TRE-API/TRE-API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --self-contained=false \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
ENV ASPNETCORE_ENVIRONMENT=Development
USER root
COPY rootca.pem /usr/local/share/ca-certificates/chi-ansible.crt
RUN chmod 644 /usr/local/share/ca-certificates/chi-ansible.crt && \
    update-ca-certificates
USER app
EXPOSE 8080
EXPOSE 443
WORKDIR /app
COPY --chown=app:app --from=build /app/publish .
ENTRYPOINT ["dotnet", "TRE-API.dll"]
