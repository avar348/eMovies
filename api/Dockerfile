FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY Directory.Build.props Directory.Packages.props EMovies.slnx ./
COPY src/EMovies.Api/EMovies.Api.csproj src/EMovies.Api/
COPY src/Modules/Movies/EMovies.Modules.Movies.Contracts/EMovies.Modules.Movies.Contracts.csproj src/Modules/Movies/EMovies.Modules.Movies.Contracts/
COPY src/Modules/Movies/EMovies.Modules.Movies/EMovies.Modules.Movies.csproj src/Modules/Movies/EMovies.Modules.Movies/
COPY src/Modules/Reviews/EMovies.Modules.Reviews/EMovies.Modules.Reviews.csproj src/Modules/Reviews/EMovies.Modules.Reviews/
RUN dotnet restore src/EMovies.Api/EMovies.Api.csproj

COPY src/ src/
RUN dotnet publish src/EMovies.Api/EMovies.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "EMovies.Api.dll"]
