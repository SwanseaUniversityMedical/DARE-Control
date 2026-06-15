FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:8.0.416 AS build
WORKDIR /src
COPY ["TREAgent/TREAgent.csproj", "TREAgent/"]
COPY ["BL/BL.csproj", "BL/"]
RUN dotnet restore "TREAgent/TREAgent.csproj"
COPY BL/ BL/
COPY TREAgent/ TREAgent/
RUN dotnet publish "TREAgent/TREAgent.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --self-contained=false \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
USER root
COPY rootca.pem /usr/local/share/ca-certificates/chi-ansible.crt
RUN chmod 644 /usr/local/share/ca-certificates/chi-ansible.crt && \
    update-ca-certificates
USER app
WORKDIR /app
COPY --chown=app:app --from=build /app/publish .
ENTRYPOINT ["dotnet", "TREAgent.dll"]
