# ជំហានទី ១: Build កម្មវិធី
FROM ://microsoft.com AS build-env
WORKDIR /app

# ផ្ទេរឯកសារ .csproj រួច Restore dependencies
COPY *.csproj ./
RUN dotnet restore

# ផ្ទេរកូដទាំងអស់ រួច Publish កម្មវិធី
COPY . ./
RUN dotnet publish -c Release -o out

# ជំហានទី ២: បង្កើត Runtime Image សម្រាប់ដំណើរការ
FROM ://microsoft.com
WORKDIR /app
COPY --from=build-env /app/out .

# កំណត់ឱ្យកម្មវិធីដើរក្នុង Production mode ជានិច្ចលើ Render
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "WebPlanner.dll"]
