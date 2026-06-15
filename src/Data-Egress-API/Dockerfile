FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:8.0.416 AS build
WORKDIR /src
COPY ["Data-Egress-API/Data-Egress-API.csproj", "Data-Egress-API/"]
COPY ["BL/BL.csproj", "BL/"]
RUN dotnet restore "Data-Egress-API/Data-Egress-API.csproj"
COPY BL/ BL/
COPY Data-Egress-API/ Data-Egress-API/
RUN dotnet publish "Data-Egress-API/Data-Egress-API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --self-contained=false \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
USER root
COPY rootca.pem /usr/local/share/ca-certificates/chi-ansible.crt
RUN chmod 644 /usr/local/share/ca-certificates/chi-ansible.crt && \
    update-ca-certificates
USER app
EXPOSE 8080
EXPOSE 443
WORKDIR /app
COPY --chown=app:app --from=build /app/publish .
ENTRYPOINT ["dotnet", "Data-Egress-API.dll"]
