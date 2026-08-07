FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/MMORPG.Shared/MMORPG.Shared.csproj src/MMORPG.Shared/
COPY src/MMORPG.Domain/MMORPG.Domain.csproj src/MMORPG.Domain/
COPY src/MMORPG.Infrastructure/MMORPG.Infrastructure.csproj src/MMORPG.Infrastructure/
COPY src/MMORPG.MasterServer/MMORPG.MasterServer.csproj src/MMORPG.MasterServer/

RUN dotnet restore src/MMORPG.MasterServer/MMORPG.MasterServer.csproj

COPY src/ src/
RUN dotnet publish src/MMORPG.MasterServer/MMORPG.MasterServer.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .
EXPOSE 7000
ENTRYPOINT ["dotnet", "MMORPG.MasterServer.dll"]
