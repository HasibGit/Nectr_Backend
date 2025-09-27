# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /src

# Copy the solution file
COPY Nectr_Backend.sln ./

# Copy the project file
COPY API/API.csproj API/

# Restore NuGet packages
RUN dotnet restore

# Copy the entire source code
COPY . .

# Build the application in Release mode
WORKDIR /src/API
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 8.0 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published application from the publish stage
COPY --from=publish /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/Data

# Expose the port that the app runs on
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Set the entry point
ENTRYPOINT ["dotnet", "API.dll"]