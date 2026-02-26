# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all csproj files preserving folder structure
COPY Zubs.WebApi/Zubs.WebApi.csproj ./Zubs.WebApi/
COPY Zubs.Application/Zubs.Application.csproj ./Zubs.Application/
COPY Zubs.Infrastructure/Zubs.Infrastructure.csproj ./Zubs.Infrastructure/
COPY Zubs.Domain/Zubs.Domain.csproj ./Zubs.Domain/

RUN dotnet restore ./Zubs.WebApi/Zubs.WebApi.csproj

COPY . .
RUN dotnet publish ./Zubs.WebApi/Zubs.WebApi.csproj -c Release -o /out

# Production stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Zubs.WebApi.dll", "--urls", "http://0.0.0.0:8080"]
