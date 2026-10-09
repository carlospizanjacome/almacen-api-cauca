FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Almacen.Shared/Almacen.Shared.csproj", "Almacen.Shared/"]
COPY ["Almacen.Api/Almacen.Api.csproj", "Almacen.Api/"]

RUN dotnet restore "Almacen.Api/Almacen.Api.csproj"

COPY . .

WORKDIR "/src/Almacen.Api"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Almacen.Api.dll"]