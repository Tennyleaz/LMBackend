# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:35792ea4ad1db051981f62b313f1be3b46b1f45cadbaa3c288cd0d3056eefb83 AS build
# Create a directory in /src
WORKDIR /src

# Copy csproj and restore as distinct layers to /src
COPY LMBackend.csproj ./
RUN dotnet restore ./LMBackend.csproj

# Copy the rest of the source to /src
COPY . ./

# Build and publish a release
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:6c4df091e4e531bb93bdbfe7e7f0998e7ced344f54426b7e874116a3dc3233ff
# Only /app will be in build image
WORKDIR /app
EXPOSE 7071
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LMBackend.dll"]
