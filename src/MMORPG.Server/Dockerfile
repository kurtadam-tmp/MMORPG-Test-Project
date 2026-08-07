FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and project files
COPY src/MMORPG.slnx ./src/
COPY src/MMORPG.Shared/MMORPG.Shared.csproj ./src/MMORPG.Shared/
COPY src/MMORPG.Domain/MMORPG.Domain.csproj ./src/MMORPG.Domain/
COPY src/MMORPG.Infrastructure/MMORPG.Infrastructure.csproj ./src/MMORPG.Infrastructure/
COPY src/MMORPG.Server/MMORPG.Server.csproj ./src/MMORPG.Server/

RUN dotnet restore ./src/MMORPG.Server/MMORPG.Server.csproj

# Copy source code and build
COPY src/ ./src/
RUN dotnet publish ./src/MMORPG.Server/MMORPG.Server.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "MMORPG.Server.dll"]
