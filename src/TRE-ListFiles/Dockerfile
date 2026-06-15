FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:8.0.416 AS build
WORKDIR /src
COPY ["TRE-ListFiles/TRE-ListFiles.csproj", "TRE-ListFiles/"]
RUN dotnet restore "TRE-ListFiles/TRE-ListFiles.csproj"
COPY TRE-ListFiles/ TRE-ListFiles/
RUN dotnet publish "TRE-ListFiles/TRE-ListFiles.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --self-contained=false \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
USER app
WORKDIR /app
COPY --chown=app:app --from=build /app/publish .
ENTRYPOINT ["dotnet", "TRE-ListFiles.dll"]
