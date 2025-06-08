# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BookStoreApi/*.csproj ./BookStoreApi/
RUN dotnet restore BookStoreApi/BookStoreApi.csproj

COPY BookStoreApi ./BookStoreApi
WORKDIR /src/BookStoreApi
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BookStoreApi.dll"]
