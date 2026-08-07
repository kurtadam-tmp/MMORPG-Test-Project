FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/MMORPG.Shared/MMORPG.Shared.csproj src/MMORPG.Shared/
COPY src/MMORPG.Domain/MMORPG.Domain.csproj src/MMORPG.Domain/
COPY src/MMORPG.Infrastructure/MMORPG.Infrastructure.csproj src/MMORPG.Infrastructure/
COPY src/MMORPG.GatewayApi/MMORPG.GatewayApi.csproj src/MMORPG.GatewayApi/

RUN dotnet restore src/MMORPG.GatewayApi/MMORPG.GatewayApi.csproj

COPY src/ src/
RUN dotnet publish src/MMORPG.GatewayApi/MMORPG.GatewayApi.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "MMORPG.GatewayApi.dll"]
