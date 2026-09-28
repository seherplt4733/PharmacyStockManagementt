FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Solution dosyasını bulup restore işlemini açıkça çalıştırıyoruz
COPY . .
RUN dotnet restore "pharmacystock.slnx"
RUN dotnet publish "pharmacystock/pharmacystock.csproj" -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "pharmacystock.dll"]
