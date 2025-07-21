# Build the application.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files to leverage Docker layer caching
COPY ["ScrabbleSharp.sln", "."]
COPY ["ScrabbleSharp.Contracts/ScrabbleSharp.Contracts.csproj", "ScrabbleSharp.Contracts/"]
COPY ["ScrabbleSharp.Engine/ScrabbleSharp.Engine.csproj", "ScrabbleSharp.Engine/"]
COPY ["ScrabbleSharp.Gateway/ScrabbleSharp.Gateway.csproj", "ScrabbleSharp.Gateway/"]
COPY ["ScrabbleSharp.Tests/ScrabbleSharp.Tests.csproj", "ScrabbleSharp.Tests/"]

# Restore dependencies
RUN dotnet restore "ScrabbleSharp.sln"

# Copy the rest of the source code
COPY . .

# Publish the application and generate a SBOM
WORKDIR "/src/ScrabbleSharp.Gateway"
RUN dotnet publish "ScrabbleSharp.Gateway.csproj" -c Release -o /app/publish --no-restore /p:GenerateCycloneDXSbom=true

# Create the final production image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Install curl for the HEALTHCHECK command.
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# For security, create and use a non-root user
RUN adduser --system --group --no-create-home appuser
USER appuser

# Copy the published application from the build stage
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ScrabbleSharp.Gateway.dll"]