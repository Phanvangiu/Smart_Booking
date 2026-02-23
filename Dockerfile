# ─── Stage 1: Build ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj trước → tận dụng layer cache
# Nếu chỉ đổi code, không đổi packages → Docker không restore lại
COPY SmartBooking.Domain/SmartBooking.Domain.csproj                   SmartBooking.Domain/
COPY SmartBooking.Application/SmartBooking.Application.csproj         SmartBooking.Application/
COPY SmartBooking.Infrastructure/SmartBooking.Infrastructure.csproj   SmartBooking.Infrastructure/
COPY SmartBooking.API/SmartBooking.API.csproj                         SmartBooking.API/

RUN dotnet restore SmartBooking.API/SmartBooking.API.csproj

# Copy source code sau khi restore
COPY SmartBooking.Domain/         SmartBooking.Domain/
COPY SmartBooking.Application/    SmartBooking.Application/
COPY SmartBooking.Infrastructure/ SmartBooking.Infrastructure/
COPY SmartBooking.API/            SmartBooking.API/

RUN dotnet publish SmartBooking.API/SmartBooking.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ─── Stage 2: Runtime ─────────────────────────────────────────────
# SDK ~800MB → Runtime ~200MB
# Stage 1 chỉ dùng để build, không đi vào image cuối
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartBooking.API.dll"]