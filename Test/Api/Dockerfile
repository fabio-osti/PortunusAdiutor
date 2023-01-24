FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Test/Api/PortunusAdiutorApiTest.csproj", "Test/Api/"]
RUN dotnet restore "Test/Api/PortunusAdiutorApiTest.csproj"
COPY ["Test/Api/", "Test/Api/"]
COPY ["PortunusAdiutor", "PortunusAdiutor"]
WORKDIR "/src/Test/Api"
RUN dotnet build "PortunusAdiutorApiTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PortunusAdiutorApiTest.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PortunusAdiutorApiTest.dll"]
