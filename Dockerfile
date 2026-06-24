# ជំហានទី ១: Build កម្មវិធី ASP.NET Core MVC + API
FROM ://microsoft.com AS build-env
WORKDIR /app

# ចម្លងឯកសារ .csproj រួច Restore dependencies
COPY *.csproj ./
RUN dotnet restore

# ចម្លងកូដ និងឯកសារ Views/wwwroot ទាំងអស់ រួច Publish កម្មវិធី
COPY . ./
RUN dotnet publish -c Release -o out

# ជំហានទី ២: បង្កើត Runtime Image សម្រាប់ដំណើរការ
FROM ://microsoft.com
WORKDIR /app
COPY --from=build-env /app/out .

# កំណត់ឱ្យកម្មវិធីដើរក្នុង Production mode ជានិច្ចលើ Render
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "WebPlanner.dll"]
