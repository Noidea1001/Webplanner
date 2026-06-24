# ជំហានទី ១: Build កម្មវិធី ASP.NET Core MVC + API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# ចម្លងឯកសារ .csproj រួច Restore dependencies
COPY *.csproj ./
RUN dotnet restore

# ចម្លងកូដទាំងអស់ រួច Publish
COPY . ./
RUN dotnet publish -c Release -o out

# ជំហានទី ២: Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Environment
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebPlanner.dll"]