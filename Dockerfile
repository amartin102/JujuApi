# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto (.csproj) y restaurar dependencias
# Esto se hace primero para aprovechar la caché de Docker
COPY ["1. Presentation/JujuApi/JujuApi.csproj", "1. Presentation/JujuApi/"]
COPY ["2. Application/Application/Application.csproj", "2. Application/Application/"]
COPY ["3. Domain/Domain/Domain.csproj", "3. Domain/Domain/"]
COPY ["4. Infraestructure/Repository/Repository.csproj", "4. Infraestructure/Repository/"]

RUN dotnet restore "1. Presentation/JujuApi/JujuApi.csproj"

# Copiar todo el resto del código y compilar
COPY . .
WORKDIR "/src/1. Presentation/JujuApi"
RUN dotnet build "JujuApi.csproj" -c Release -o /app/build

# Etapa 2: Publish
FROM build AS publish
RUN dotnet publish "JujuApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JujuApi.dll"]