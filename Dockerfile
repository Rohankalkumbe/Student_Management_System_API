# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["StudentManagementSystem.csproj", "./"]
RUN dotnet restore "StudentManagementSystem.csproj"

COPY . .
RUN dotnet publish "StudentManagementSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "StudentManagementSystem.dll"]
