FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["WebPlanner.csproj", "./"]
RUN dotnet restore "WebPlanner.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "WebPlanner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebPlanner.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebPlanner.dll"]