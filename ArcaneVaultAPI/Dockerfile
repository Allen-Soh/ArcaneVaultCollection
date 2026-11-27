# ArcaneVaultAPI — container image for Render (free web service)
# Build context: the ArcaneVaultAPI folder (see render.yaml)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ArcaneVaultAPI.csproj .
RUN dotnet restore ArcaneVaultAPI.csproj
COPY . .
RUN dotnet publish ArcaneVaultAPI.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# The controllers open "Data Source=ArcaneVault.db" relative to the working directory,
# so the committed database (plus its WAL/SHM files, which may hold recent writes) must sit in /app.
# NOTE: Render's free tier has no persistent disk — anything written at runtime is lost
# whenever the service restarts, sleeps, or redeploys, and the DB resets to this committed copy.
COPY ArcaneVault.db* ./

# Render routes traffic to port 10000 by default; TLS is terminated by Render's proxy.
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "ArcaneVaultAPI.dll"]
