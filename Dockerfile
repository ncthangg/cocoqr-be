# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy solution file first for better caching
COPY CocoQR.sln .

# Copy project files separately for better cache utilization
COPY CocoQR.API/CocoQR.API.csproj CocoQR.API/
COPY CocoQR.Application/CocoQR.Application.csproj CocoQR.Application/
COPY CocoQR.Domain/CocoQR.Domain.csproj CocoQR.Domain/
COPY CocoQR.Infrastructure/CocoQR.Infrastructure.csproj CocoQR.Infrastructure/
COPY CocoQR.QR_Generator/CocoQR.QR_Generator.csproj CocoQR.QR_Generator/

# Restore dependencies (cached unless .csproj files change)
RUN dotnet restore CocoQR.API/CocoQR.API.csproj

# Copy source code (only invalidates cache when code changes)
COPY CocoQR.API/ CocoQR.API/
COPY CocoQR.Application/ CocoQR.Application/
COPY CocoQR.Domain/ CocoQR.Domain/
COPY CocoQR.Infrastructure/ CocoQR.Infrastructure/
COPY CocoQR.QR_Generator/ CocoQR.QR_Generator/

# Publish
RUN dotnet publish CocoQR.API/CocoQR.API.csproj \
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
ENTRYPOINT ["dotnet", "CocoQR.API.dll"]
