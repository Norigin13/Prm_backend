# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

# Run
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
# Render tự động inject PORT env, listen trên 0.0.0.0
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
EXPOSE ${PORT:-8080}
ENTRYPOINT ["dotnet", "PRM_Backend.dll"]