# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SitAiInterface.csproj .
RUN dotnet restore SitAiInterface.csproj

COPY . .
RUN dotnet publish SitAiInterface.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Copy the knowledge base into the image
COPY --from=build /src/sit-scraped/ sit-scraped/

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SitAiInterface.dll"]
