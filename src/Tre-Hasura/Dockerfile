FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:8.0.416 AS build
WORKDIR /src
COPY ["Tre-Hasura/Tre-Hasura.csproj", "Tre-Hasura/"]
COPY ["BL/BL.csproj", "BL/"]
RUN dotnet restore "Tre-Hasura/Tre-Hasura.csproj"
COPY BL/ BL/
COPY Tre-Hasura/ Tre-Hasura/
RUN dotnet publish "Tre-Hasura/Tre-Hasura.csproj" \
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
ENTRYPOINT ["dotnet", "Tre-Hasura.dll"]
