# eMovies API

ASP.NET Core modular-monolith API using PostgreSQL, Keycloak JWT authentication,
Entity Framework Core, and AutoMapper. It currently contains Movies, Reviews,
and Users modules.

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
    ├── Reviews/EMovies.Modules.Reviews/
        ├── Domain/                      # Module entities and invariants
        ├── Application/                 # Models, mapping, and use cases
        ├── Infrastructure/              # PostgreSQL/EF Core
        └── Presentation/                # API controller and authorization
    └── Users/EMovies.Modules.Users/
        ├── Domain/                      # User profile and onboarding rules
        ├── Application/                 # Profile use cases
        ├── Infrastructure/              # users schema and EF migrations
        └── Presentation/                # Authenticated profile endpoints
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
`localhost:5432`. Swagger UI is available at
`http://localhost:5000/swagger`.

The local development identities are:

- Keycloak Admin Console administrator (`master` realm):
  `admin` / `admin_dev_password`
- eMovies application administrator used by the current local test environment
  (`emovies` realm):
  `admin.user` / `admin_dev_password`
- Clean-realm seeded API identity: `demo` / `demo_dev_password`

The Keycloak administrator manages realms, clients, roles, and identities at
`http://localhost:8080/admin`. It is not the account used to test the eMovies
administration dashboard. Use `admin.user` through the frontend login flow to
review library-manager requests. The bundled realm import seeds `demo` with
application-admin roles; `admin.user` is the dedicated administrator in the
current retained development realm.

These credentials and development-mode settings must be replaced outside local
development.

Self-registration is enabled in the bundled `emovies` realm. New users receive
the `movies-reader` role automatically and can sign up through the frontend's
**Create an account** link. The Users module stores the application profile in
its own `users` schema. Choosing a library-manager account creates a
`PendingApproval` profile. When an administrator approves that profile, the API
assigns the `movies-manager` Keycloak role. Managers can access catalog,
inventory, rental, and review-moderation operations, while `movies-admin`
remains reserved for administrator-only work such as approving or denying
manager requests.

## Roles and permissions

Keycloak roles grant system permissions. The Users module profile tracks the
product onboarding state, such as whether a library-manager request is still
pending, active, or denied.

| Role | Intended user | Permissions |
| --- | --- | --- |
| `movies-reader` | Movie renter | Browse the movie catalog, view movie details, and use renter-facing movie features. This is the default role for self-registered users. |
| `movies-manager` | Approved library manager | Includes movie-reader access, plus catalog and inventory management, rental operations, and review moderation. Assigned by the API when an admin approves a pending library-manager profile. Removed by the API when a pending manager request is denied. |
| `reviews-moderator` | Review moderator | Moderate movie reviews without granting catalog, inventory, rental, or user-approval permissions. |
| `movies-admin` | Administrator | Full administrative access, including manager approvals. Admins can do manager-level operations, but managers cannot approve or deny other managers. |

Current API policies:

| Policy | Allowed roles |
| --- | --- |
| `movies.read` | `movies-reader`, `movies-manager`, `movies-admin` |
| `movies.write` | `movies-manager`, `movies-admin` |
| `reviews.moderate` | `reviews-moderator`, `movies-manager`, `movies-admin` |
| `users.manage-approvals` | `movies-admin` |

Legacy local realm roles are still accepted for compatibility:
`emovies-member` maps to movie-reader behavior, `emovies-staff` maps to
review-moderator behavior, and `emovies-admin` maps to admin behavior.

The local `admin.user` identity must have both `movies-reader` and
`movies-admin`. The reader role allows the login callback to validate catalog
access, while the admin role enables the approval API and administration
workspace. An administrator automatically receives manager-level API
permissions; it does not need the `movies-manager` role.

## Administrator flow

1. Sign out of any renter or pending-manager session in the frontend.
2. Continue to Keycloak and sign in with:

   ```text
   Username: admin.user
   Password: admin_dev_password
   ```

3. After Keycloak redirects to the frontend, the dashboard displays:
   - **Administration → Pending manager approvals**
   - The number of pending library-manager requests
   - **Approve** and **Deny** actions for each pending request
   - The manager-level **Library operations** workspace

Approving a request changes its Users module profile from `PendingApproval` to
`Active` and assigns the `movies-manager` Keycloak role. Denying a request
changes the profile to `Denied` and removes that role. A pending manager can
use renter-facing features but cannot see manager tools before approval.

Keycloak only imports a realm when it does not already exist. If your local
`emovies` realm was created before self-registration was enabled, either turn on
**Realm settings → Login → User registration** and add `movies-reader` to the
realm's default roles in the Admin Console, or recreate the local Keycloak data
volume before starting the stack again.

For an existing local realm, also verify that:

- Realm roles include `movies-reader`, `movies-manager`, and `movies-admin`.
- `admin.user` has `movies-reader` and `movies-admin`.
- New self-registered identities receive `movies-reader`.

If the administration panel says pending approvals are unavailable, call
`GET /api/users/pending-approvals` without a token. A current API build returns
`401 Unauthorized`. A `409 Conflict` stating that
`users.manage-approvals` was not found means the running API image predates the
Users module authorization policy. Rebuild and restart the API from the current
source, then sign out and back in so Keycloak issues a fresh token.

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

See [Roles and permissions](#roles-and-permissions) for the current role and
policy model.

### Authenticate in Swagger UI

Open `http://localhost:5000/swagger`, select **Authorize**, and choose the
available `openid` and `profile` scopes. Swagger redirects to Keycloak using the
Authorization Code flow with PKCE.

For local development, sign in with:

```text
Username: demo
Password: demo_dev_password
```

After authorization, Swagger automatically includes the access token when
executing protected endpoints. The health endpoint remains anonymous. Swagger
UI is enabled only in the Development environment by default.

If the `emovies` realm was imported before Swagger support was added, update the
`emovies-client` in the Keycloak Admin Console with:

```text
Valid redirect URI: http://localhost:5000/swagger/oauth2-redirect.html
Web origin:         http://localhost:5000
```

Keycloak skips startup imports for realms that already exist, so editing the
realm JSON does not update an existing realm automatically.

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

Development mode automatically applies pending migrations for all modules when
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

Replace the context and module path to run the same commands for
`MoviesDbContext` or `UsersDbContext`.

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
ConnectionStrings__UsersDatabase=...
Keycloak__Authority=https://identity.example.com/realms/emovies
Keycloak__Audience=emovies-api
Keycloak__RequireHttpsMetadata=true
Movies__MigrateOnStartup=false
Reviews__MigrateOnStartup=false
Users__MigrateOnStartup=false
AutoMapper__LicenseKey=...
```

Run all checks with:

```bash
dotnet test EMovies.slnx
```
