FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:8.0.416 AS build
WORKDIR /src
COPY ["TRE-UI/TRE-UI.csproj", "TRE-UI/"]
COPY ["BL/BL.csproj", "BL/"]
RUN dotnet restore "TRE-UI/TRE-UI.csproj"
COPY BL/ BL/
COPY TRE-UI/ TRE-UI/
RUN dotnet publish "TRE-UI/TRE-UI.csproj" \
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
ENTRYPOINT ["dotnet", "TRE-UI.dll"]
