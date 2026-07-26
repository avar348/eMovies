# eMovies API

ASP.NET Core modular-monolith API using PostgreSQL, Keycloak JWT authentication,
Entity Framework Core, and AutoMapper. It currently contains Movies and Reviews
modules.

## Architecture

`EMovies.Api` is the composition root. Business functionality lives in modules,
and the host only wires modules and cross-cutting concerns together.

```text
src/
├── EMovies.Api/                         # Host, authentication, middleware
└── Modules/
    ├── Movies/
    │   ├── EMovies.Modules.Movies.Contracts/ # Cross-module movie lookup
    │   └── EMovies.Modules.Movies/
    └── Reviews/EMovies.Modules.Reviews/
        ├── Domain/                      # Module entities and invariants
        ├── Application/                 # Models, mapping, and use cases
        ├── Infrastructure/              # PostgreSQL/EF Core
        └── Presentation/                # API controller and authorization
```

Each future business area should be added as another module and own its database
schema, services, models, and controllers.

## Run locally

Requirements: .NET 10 SDK and Docker.

```bash
cd api
docker compose up -d
dotnet run --project src/EMovies.Api
```

Development mode automatically applies EF Core migrations. The API listens on
`http://localhost:5000`, Keycloak on `http://localhost:8080`, and PostgreSQL on
`localhost:5432`.

The included development identities are:

- Keycloak admin: `admin` / `admin_dev_password`
- API demo user: `demo` / `demo_dev_password`

These credentials and development-mode settings must be replaced outside local
development.

## Authenticate

Request a development access token:

```bash
curl -s \
  -d "client_id=emovies-client" \
  -d "username=demo" \
  -d "password=demo_dev_password" \
  -d "grant_type=password" \
  http://localhost:8080/realms/emovies/protocol/openid-connect/token
```

Copy the `access_token` value, then call an endpoint:

```bash
curl -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  http://localhost:5000/api/movies
```

The `movies-reader` realm role can read movies. The `movies-admin` role can
create, update, and delete them. Any authenticated user can submit one review
per movie. The `reviews-moderator` or `movies-admin` role can moderate reviews.

## Postman

Import both files into Postman:

- `postman/eMovies.postman_collection.json`
- `postman/eMovies.postman_environment.json`

Select the **eMovies Local** environment and run the **eMovies API** collection
in its numbered order. The collection automatically:

1. Checks API health.
2. Authenticates with Keycloak and saves `accessToken`.
3. Creates a movie and saves `movieId`.
4. Exercises movie reads and updates.
5. Submits a review and saves `reviewId`.
6. Moderates the review and checks the average rating.
7. Updates and re-approves the review.
8. Deletes the example review and movie.

Start the API, PostgreSQL, and Keycloak before running the collection:

```bash
docker compose up -d
dotnet run --project src/EMovies.Api
```

The `IsFeatured` and `ContainsSpoilers` fields in the migration section are
documentation examples and are not included in the Postman requests until those
fields are implemented in the API models.

## Example create request

```bash
curl -X POST http://localhost:5000/api/movies \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "The Matrix",
    "description": "A computer hacker discovers the truth.",
    "releaseDate": "1999-03-31",
    "genre": "Science Fiction"
  }'
```

## Reviews

Submit a rating with an optional written review:

```bash
curl -X POST http://localhost:5000/api/movies/MOVIE_ID/reviews \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "rating": 5,
    "content": "Excellent movie."
  }'
```

New and edited reviews are `Pending` until approved. Review lists and average
ratings include only approved reviews.

```text
GET    /api/movies/{movieId}/reviews
GET    /api/movies/{movieId}/rating
POST   /api/movies/{movieId}/reviews
PUT    /api/movies/{movieId}/reviews/me
DELETE /api/movies/{movieId}/reviews/me
GET    /api/reviews/moderation?status=Pending
PATCH  /api/reviews/moderation/{reviewId}
```

Approve a review with `{ "approve": true }`. Reject one with
`{ "approve": false, "reason": "Reason for rejection" }`.

## Database migrations

Each module owns its `DbContext`, migration folder, PostgreSQL schema, and
migration history. Run all migration commands from the `api` directory.

Development mode automatically applies pending migrations for both modules when
the API starts:

```bash
docker compose up -d
dotnet run --project src/EMovies.Api
```

For production or manual updates, create and apply each module's migrations
independently.

### Example: add a boolean to Movies

For example, add an `IsFeatured` property to the `Movie` entity:

```csharp
public bool IsFeatured { get; private set; }
```

Then create the Movies migration:

```bash
dotnet ef migrations add AddMovieIsFeatured \
  --project src/Modules/Movies/EMovies.Modules.Movies \
  --startup-project src/EMovies.Api \
  --context MoviesDbContext \
  --output-dir Infrastructure/Persistence/Migrations
```

Apply only the Movies migrations:

```bash
dotnet ef database update \
  --project src/Modules/Movies/EMovies.Modules.Movies \
  --startup-project src/EMovies.Api \
  --context MoviesDbContext
```

The generated migration will contain a change similar to:

```csharp
migrationBuilder.AddColumn<bool>(
    name: "IsFeatured",
    schema: "movies",
    table: "movies",
    type: "boolean",
    nullable: false,
    defaultValue: false);
```

### Example: add a boolean to Reviews

For example, add a `ContainsSpoilers` property to the `Review` entity:

```csharp
public bool ContainsSpoilers { get; private set; }
```

Add the corresponding property to `SubmitReviewRequest`,
`UpdateReviewRequest`, and `ReviewResponse`, then update the entity's `Create`
and `Update` methods.

Create the Reviews migration:

```bash
dotnet ef migrations add AddReviewContainsSpoilers \
  --project src/Modules/Reviews/EMovies.Modules.Reviews \
  --startup-project src/EMovies.Api \
  --context ReviewsDbContext \
  --output-dir Infrastructure/Persistence/Migrations
```

Apply only the Reviews migrations:

```bash
dotnet ef database update \
  --project src/Modules/Reviews/EMovies.Modules.Reviews \
  --startup-project src/EMovies.Api \
  --context ReviewsDbContext
```

The generated migration will contain a change similar to:

```csharp
migrationBuilder.AddColumn<bool>(
    name: "ContainsSpoilers",
    schema: "reviews",
    table: "reviews",
    type: "boolean",
    nullable: false,
    defaultValue: false);
```

### Inspect and manage migrations

List migrations for a module:

```bash
dotnet ef migrations list \
  --project src/Modules/Reviews/EMovies.Modules.Reviews \
  --startup-project src/EMovies.Api \
  --context ReviewsDbContext
```

Check whether the model has changes that are not represented by a migration:

```bash
dotnet ef migrations has-pending-model-changes \
  --project src/Modules/Reviews/EMovies.Modules.Reviews \
  --startup-project src/EMovies.Api \
  --context ReviewsDbContext
```

Remove the latest migration before it has been applied:

```bash
dotnet ef migrations remove \
  --project src/Modules/Reviews/EMovies.Modules.Reviews \
  --startup-project src/EMovies.Api \
  --context ReviewsDbContext
```

Replace `ReviewsDbContext` and the Reviews project path with
`MoviesDbContext` and the Movies project path to run the same commands for the
Movies module.

To target another database, provide the module's connection string:

```bash
ConnectionStrings__ReviewsDatabase="Host=...;Database=...;Username=...;Password=..." \
dotnet ef database update \
  --project src/Modules/Reviews/EMovies.Modules.Reviews \
  --startup-project src/EMovies.Api \
  --context ReviewsDbContext
```

Review every generated migration before applying it, especially changes that
drop columns, rename data, or add non-null columns to tables containing data.

Production configuration should provide secrets through environment variables,
for example:

```text
ConnectionStrings__MoviesDatabase=...
ConnectionStrings__ReviewsDatabase=...
Keycloak__Authority=https://identity.example.com/realms/emovies
Keycloak__Audience=emovies-api
Keycloak__RequireHttpsMetadata=true
Movies__MigrateOnStartup=false
Reviews__MigrateOnStartup=false
AutoMapper__LicenseKey=...
```

Run all checks with:

```bash
dotnet test EMovies.slnx
```
