FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
WORKDIR /src
COPY Comms-Server/Comms-Server.csproj Comms-Server/
RUN dotnet restore Comms-Server/Comms-Server.csproj
ENTRYPOINT ["dotnet", "watch", "--project", "Comms-Server/Comms-Server.csproj", "run", "--no-launch-profile"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG VERSION=""
WORKDIR /src
COPY . .
RUN dotnet publish Comms-Server/Comms-Server.csproj -c Release -o /app/publish ${VERSION:+/p:MinVerVersionOverride=$VERSION}

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Comms-Server.dll"]
