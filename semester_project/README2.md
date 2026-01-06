# Media Ratings Platform (MRP)

A standalone RESTful HTTP server (no ASP.NET) that provides an API for managing media (movies/series/games), rating and moderating comments, liking ratings, favorites, recommendations, and a public leaderboard. This project follows the “Media Ratings Platform (MRP)” specification.

The server listens on:

- `http://localhost:8080/`
- API base path: `http://localhost:8080/api`

---

## Features

- User registration and login (token-based auth)
- User profile (view/update) and personal statistics
- Media CRUD (create, update, delete, get by id)
- Media search with filtering and sorting
- Ratings (1–5 stars) with optional comments
- Comment moderation: comments become publicly visible only after the author confirms them
- Like other users’ ratings (one like per rating per user)
- Favorite / unfavorite media
- Rating history & favorites list per user
- Recommendations based on prior ratings and content similarity
- Leaderboard of most active users

---

## Tech Stack

- **Language/Runtime:** C# / .NET
- **HTTP Server:** custom REST server using `HttpListener`-style approach (no ASP.NET)
- **Database:** PostgreSQL (can be run via Docker)
- **Testing:** unit tests in `Tests/`

---

## Project Structure (Clean Architecture)

High-level structure (layers + tests):

- `semester_project/1Presentation`  
  HTTP server, routing, controllers, DTOs (request/response contracts)
- `semester_project/2Application`  
  Use cases (handlers), application interfaces (repositories/services)
- `semester_project/3Domain`  
  Domain entities and enums
- `semester_project/4Infrastructure`  
  PostgreSQL repositories, connection factory, token service
- `Tests/`  
  Unit tests for core business logic (handlers)

---

## Getting Started

### Prerequisites

- **.NET SDK** compatible with the project (see `semester_project/semester_project.csproj`)
- **PostgreSQL**
  - either installed locally, or
  - run via Docker using `semester_project/docker-compose.yml`

### Database Setup

1. Start PostgreSQL (Docker or local).
2. Initialize the schema using the provided SQL:
   - `schema.sql` exists at the repository root and also under `semester_project/schema.sql`.

Example (if you have `psql` locally):

```bash
psql -h localhost -U <user> -d <database> -f schema.sql
```

If you use Docker Compose, check `semester_project/docker-compose.yml` for the configured database name/user/password/port, then apply the schema accordingly.

### Run the Server

From the repository root:

```bash
dotnet run --project semester_project/semester_project.csproj
```

Once started, the server listens on `http://localhost:8080/` and exposes routes under `/api`.

---

## Authentication

This project uses **token-based authorization**.

- **Public endpoints:** registration and login
- **All other endpoints:** require a bearer token

Use this header on authenticated requests:

- `Authorization: Bearer <token>`

### Login Example

```bash
curl -s -X POST "http://localhost:8080/api/users/login" \
  -H "Content-Type: application/json" \
  -d '{ "username": "mustermann", "password": "max" }'
```

The response returns a token (string). Use it in subsequent requests:

```bash
curl -s "http://localhost:8080/api/leaderboard" \
  -H "Authorization: Bearer <token>"
```

---

## API Routes

These are the implemented routes:

### Leaderboard

- `GET /api/leaderboard` → LeaderboardController.Get()

### Media

- `POST /api/media` → MediaController.Create()
- `GET /api/media` → MediaController.Search()
- `GET /api/media/{id}` → MediaController.GetById()
- `PUT /api/media/{id}` → MediaController.Update()
- `DELETE /api/media/{id}` → MediaController.Delete()

#### Media Search Query Parameters

Supported query parameters (combine as needed):

- `title` (partial match)
- `genre`
- `mediaType`
- `releaseYear`
- `ageRestriction`
- `rating`
- `sortBy` (`score` | `title` | `year`)

Examples:

```bash
curl -s "http://localhost:8080/api/media?title=incep&genre=sci-fi&sortBy=score" \
  -H "Authorization: Bearer <token>"
```

```bash
curl -s "http://localhost:8080/api/media?title=inception&genre=sci-fi&mediaType=movie&releaseYear=2010&ageRestriction=12&rating=4&sortBy=title" \
  -H "Authorization: Bearer <token>"
```

### Ratings

- `POST /api/media/{id}/rate` → MediaController.CreateForMedia()
- `PUT /api/ratings/{id}` → RatingsController.Update()
- `DELETE /api/ratings/{id}` → RatingsController.Delete()
- `POST /api/ratings/{id}/confirm` → RatingsController.Approve()
- `POST /api/ratings/{id}/like` → RatingsController.LikeRating()

### Favorites

- `POST /api/media/{id}/favorite` → MediaController.MarkMediaAsFavorite()
- `DELETE /api/media/{id}/favorite` → MediaController.UnmarkMediaAsFavorite()
- `GET /api/users/{id}/favorites` → UsersController.GetFavorites()

### Users / Profile / History / Recommendations

- `POST /api/users/register` → UsersController.Register()
- `POST /api/users/login` → UsersController.Login()
- `GET /api/users/{id}/profile` → UsersController.GetProfile()
- `PUT /api/users/{id}/profile` → UsersController.UpdateProfile()
- `GET /api/users/{id}/ratings` → UsersController.GetRatingHistory()
- `GET /api/users/{id}/recommendations` → UsersController.Get()

---

## Response Codes

The API aims to follow standard HTTP semantics:

- `2XX` success
- `4XX` client error (bad input, missing/invalid auth, forbidden actions)
- `5XX` server error (unexpected failures)

---

## Unit Tests

Unit tests are located in `Tests/`.

Run all tests:

```bash
dotnet test Tests/Tests.csproj
```

---

## Notes on Design

- **Layering:** Presentation → Application → Domain → Infrastructure, to keep HTTP/DTO concerns separate from business rules and persistence.
- **Persistence:** repositories under `4Infrastructure/repositories` encapsulate PostgreSQL access.
- **Security:** token service + bearer tokens protect all endpoints except register/login.
- **Moderation:** rating comments are only publicly visible after user confirmation (author moderation).

---

## Quick cURL Cookbook

Register:

```bash
curl -s -X POST "http://localhost:8080/api/users/register" \
  -H "Content-Type: application/json" \
  -d '{ "username": "alice", "password": "secret" }'
```

Login:

```bash
TOKEN=$(curl -s -X POST "http://localhost:8080/api/users/login" \
  -H "Content-Type: application/json" \
  -d '{ "username": "alice", "password": "secret" }')
echo "$TOKEN"
```

Create media:

```bash
curl -s -X POST "http://localhost:8080/api/media" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Inception",
    "description": "A mind-bending thriller.",
    "mediaType": "movie",
    "releaseYear": "2010",
    "genres": ["sci-fi", "thriller"],
    "ageRestriction": 12
  }'
```

Rate media:

```bash
curl -s -X POST "http://localhost:8080/api/media/1/rate" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{ "stars": 5, "comment": "Excellent." }'
```

Confirm rating comment (example rating id = 10):

```bash
curl -s -X POST "http://localhost:8080/api/ratings/10/confirm" \
  -H "Authorization: Bearer <token>"
```

---

## License

Educational project (MRP assignment). Add a license here if your course or repository requires one.
