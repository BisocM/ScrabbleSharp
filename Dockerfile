FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer caching
COPY ["ScrabbleSharp.sln", "."]
COPY ["ScrabbleSharp.Contracts/ScrabbleSharp.Contracts.csproj", "ScrabbleSharp.Contracts/"]
COPY ["ScrabbleSharp.Engine/ScrabbleSharp.Engine.csproj", "ScrabbleSharp.Engine/"]
COPY ["ScrabbleSharp.Gateway/ScrabbleSharp.Gateway.csproj", "ScrabbleSharp.Gateway/"]

# Restore NuGet packages for all projects in the solution
RUN dotnet restore "ScrabbleSharp.sln"

# Copy the rest of the source code
COPY . .

# Publish the Gateway project for release
WORKDIR "/src/ScrabbleSharp.Gateway"
RUN dotnet publish "ScrabbleSharp.Gateway.csproj" -c Release -o /app/publish --no-restore

# Use the lightweight ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# For security, create a non-root user to run the application.
RUN adduser --system --group --no-create-home appuser
USER appuser

# Copy the published output from the build stage.
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "ScrabbleSharp.Gateway.dll"]