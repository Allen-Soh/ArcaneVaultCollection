# ArcaneVaultRP — container image for Render (free web service)
# Build context: the ArcaneVaultRP folder (see render.yaml)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ArcaneVaultRP.csproj .
RUN dotnet restore ArcaneVaultRP.csproj
COPY . .
RUN dotnet publish ArcaneVaultRP.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# Render routes traffic to port 10000 by default; TLS is terminated by Render's proxy.
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "ArcaneVaultRP.dll"]
