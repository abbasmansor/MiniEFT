# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# restore first (layer cache)
COPY MiniEft.sln ./
COPY src/MiniEft.Domain/*.csproj          src/MiniEft.Domain/
COPY src/MiniEft.Application/*.csproj     src/MiniEft.Application/
COPY src/MiniEft.Infrastructure/*.csproj  src/MiniEft.Infrastructure/
COPY src/MiniEft.Api/*.csproj             src/MiniEft.Api/
RUN dotnet restore src/MiniEft.Api/MiniEft.Api.csproj

COPY src/ src/
RUN dotnet publish src/MiniEft.Api/MiniEft.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MiniEft.Api.dll"]
