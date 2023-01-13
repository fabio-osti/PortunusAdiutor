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
COPY ["Tester/Link/PortunusLinkExample.csproj", "Tester/Link/"]
RUN dotnet restore "Tester/Link/PortunusLinkExample.csproj"
COPY . .
WORKDIR "/src/Tester/Link"
RUN dotnet build "PortunusLinkExample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PortunusLinkExample.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PortunusLinkExample.dll"]
