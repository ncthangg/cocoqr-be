# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy solution file first for better caching
COPY MyWallet_BE.sln .

# Copy project files separately for better cache utilization
COPY MyWallet.API/MyWallet.API.csproj MyWallet.API/
COPY MyWallet.Application/MyWallet.Application.csproj MyWallet.Application/
COPY MyWallet.Domain/MyWallet.Domain.csproj MyWallet.Domain/
COPY MyWallet.Infrastructure/MyWallet.Infrastructure.csproj MyWallet.Infrastructure/
COPY MyWallet.QR_Generator/MyWallet.QR_Generator.csproj MyWallet.QR_Generator/

# Restore dependencies (cached unless .csproj files change)
RUN dotnet restore MyWallet.API/MyWallet.API.csproj

# Copy source code (only invalidates cache when code changes)
COPY MyWallet.API/ MyWallet.API/
COPY MyWallet.Application/ MyWallet.Application/
COPY MyWallet.Domain/ MyWallet.Domain/
COPY MyWallet.Infrastructure/ MyWallet.Infrastructure/
COPY MyWallet.QR_Generator/ MyWallet.QR_Generator/

# Publish
RUN dotnet publish MyWallet.API/MyWallet.API.csproj \
    -c Release \
    -o /app/publish 

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# CỰC KỲ QUAN TRỌNG với Alpine
RUN apk add --no-cache \
    icu-libs \
    tzdata

# Bật globalization cho .NET
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Set timezone (VN)
ENV TZ=Asia/Ho_Chi_Minh

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "MyWallet.API.dll"]
