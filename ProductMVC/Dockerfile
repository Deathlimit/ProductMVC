FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8097

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["productsMVC.sln", "."]
COPY ["global.json", "."]
COPY ["ProductMVC/ProductMVC.csproj", "ProductMVC/"]
RUN dotnet restore "ProductMVC/ProductMVC.csproj"
COPY . .
WORKDIR "/src/ProductMVC"
RUN dotnet publish "./ProductMVC.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8097
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProductMVC.dll"]
