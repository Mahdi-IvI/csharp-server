# PROTOCOL — Development Report (MRP)

This document summarizes key technical steps, architecture decisions, unit test approach, issues encountered, and time tracking. The git history is considered part of the documentation.

## 1. Architecture & Technical Decisions

### 1.1 Layered/Clean Architecture
The solution is organized into four layers, enabling separation of concerns and testability:

- **1Presentation**: HTTP server, controllers, routing, JSON DTOs
- **2Application**: business use cases (handlers), inputs/results, repository interfaces
- **3Domain**: entities and enums (pure domain model)
- **4Infrastructure**: PostgreSQL repositories, connection factory, token service

This structure keeps business logic independent from transport (HTTP) and storage (PostgreSQL).

### 1.2 Custom HTTP Server & Routing
- Implemented a lightweight HTTP server, with attribute-based routing in the presentation layer.
- Controllers map HTTP requests to use case handlers.
- DTOs isolate HTTP contracts from domain and application models.

### 1.3 Persistence (PostgreSQL)
- PostgreSQL is used as the system of record, as required.
- Schema covers: users, media, ratings, likes, favorites, and media genre tags.
- Repositories implement application interfaces (ports), using Npgsql and explicit SQL.

### 1.4 Token-Based Authorization
- Implemented token-based authorization where every request (except register/login and leaderboard) must include a Bearer token header.
- Presentation layer pre-checks tokens and extracts the username before calling handlers.
- Authorization decisions:
  - Only the media creator can update/delete that media.
  - Users can only edit/delete their own ratings.
  - Rating likes are limited to one per user per rating.

### 1.5 Moderation for Rating Comments
- Rating comments are not publicly visible until confirmation.  
- Implemented a dedicated endpoint to confirm comments:
  - `POST /api/ratings/{id}/confirm`

### 1.6 Search/Filter/Sort Strategy
Media search supports:
- partial title matching
- filters (genre, media type, release year, age restriction, rating)
- sorting by title/year/score

The query is implemented server-side to keep the API fast and consistent for any frontend.

---

## 2. Unit Test Coverage & Rationale

### 2.1 What is Tested
Unit tests focus on **core business logic** at the handler/use-case level (application layer), avoiding HTTP and database integration complexity. This aligns with the requirement to validate core logic with a meaningful unit test suite.

Covered areas include:
- **User use cases**: registration, login, profile retrieval/update, username lookups
- **Media use cases**: create/update/delete validation, ownership checks, allowed media types, genre normalization
- Input validation and expected exceptions (e.g., missing title, invalid mediaType)
- Authorization rules (forbidden operations)

### 2.2 Test Project Organization
Tests are organized by feature area:
- `Tests/Media/*`
- `Tests/Users/*`

### 2.3 Why This Approach
- The application layer is stable and deterministic, making it ideal for unit testing.
- Repository interfaces can be mocked/faked to test handler logic without requiring a running database.
- Keeps feedback fast and reduces flakiness.

---

## 3. Problems Encountered & Resolutions

### 3.1 SQL Parameter and Type Issues
**Problem:** PostgreSQL/Npgsql can throw runtime errors when parameter types are inferred incorrectly or when using the wrong execution method for a command.  
**Resolution:** Ensured correct parameter usage patterns and used the appropriate command execution method (`ExecuteNonQuery` for DELETE/UPDATE, `ExecuteScalar` only when returning a value).

### 3.2 Composite Key Constraints
**Problem:** Tables like `favorites` and `rating_likes` use composite primary keys; duplicate inserts can violate constraints.  
**Resolution:** Enforced “one favorite per user per media” and “one like per user per rating” at both:
- database level (primary keys)
- application level (idempotent behaviors / conflict-safe inserts where appropriate)

### 3.3 Authorization Edge Cases
**Problem:** Ownership checks for update/delete operations must be consistent (media creator vs. requester).  
**Resolution:** Centralized checks in handlers (application layer) by reading creator IDs from the repository before mutating state.

### 3.4 Comment Moderation Workflow
**Problem:** Comments must not be publicly visible until confirmed by the author.
**Resolution:** Added an explicit confirm endpoint and stored a boolean flag (`is_comment_confirmed`) in the ratings table.

---