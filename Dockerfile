# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy the solution file and project files
COPY *.sln .
COPY TaskManagement/*.csproj ./TaskManagement/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Publish the application
WORKDIR /source/TaskManagement
RUN dotnet publish -c release -o /app --no-restore

# Use the official ASP.NET Core runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app .

# Expose port 80 and 443
EXPOSE 80
EXPOSE 443

# Set the entrypoint
ENTRYPOINT ["dotnet", "TaskManagement.dll"]
