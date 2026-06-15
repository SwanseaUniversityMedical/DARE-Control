FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:8.0.416 AS build
WORKDIR /src
COPY ["Tre-SQL-PG/Tre-SQL-PG.csproj", "Tre-SQL-PG/"]
RUN dotnet restore "Tre-SQL-PG/Tre-SQL-PG.csproj"
COPY Tre-SQL-PG/ Tre-SQL-PG/
RUN dotnet publish "Tre-SQL-PG/Tre-SQL-PG.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --self-contained=false \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
USER app
WORKDIR /app
COPY --chown=app:app --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tre-SQL-PG.dll"]
