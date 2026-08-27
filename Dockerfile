FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY Directory.Build.props global.json ./
COPY src/RoleBasedRecords.Domain/RoleBasedRecords.Domain.csproj src/RoleBasedRecords.Domain/
COPY src/RoleBasedRecords.Application/RoleBasedRecords.Application.csproj src/RoleBasedRecords.Application/
COPY src/RoleBasedRecords.Infrastructure/RoleBasedRecords.Infrastructure.csproj src/RoleBasedRecords.Infrastructure/
COPY src/RoleBasedRecords.Api/RoleBasedRecords.Api.csproj src/RoleBasedRecords.Api/

RUN dotnet restore src/RoleBasedRecords.Api/RoleBasedRecords.Api.csproj

COPY src/ src/

RUN dotnet publish src/RoleBasedRecords.Api/RoleBasedRecords.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER $APP_UID
ENTRYPOINT ["dotnet", "RoleBasedRecords.Api.dll"]
