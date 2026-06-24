# =============================================
# Build Stage
# =============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Clean any lingering artifacts (extra safety)
RUN find . -type d -name "bin" -prune -exec rm -rf {} \; && \
    find . -type d -name "obj" -prune -exec rm -rf {} \;

# Copy csproj and restore (this layer is cached)
COPY *.csproj ./
RUN dotnet restore

# Copy source code and publish
COPY . ./
RUN dotnet publish -c Release -o out --no-restore

# =============================================
# Runtime Stage
# =============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build-env /app/out .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebPlanner.dll"]