1) Goal (current scope)

I’m building a small HTTP server in C# that exposes a REST API for a media-rating platform. Right now I’ve focused on clean layering and the request/response flow, plus the very first endpoints:
•	POST  /api/users/register → returns a token "{username}-mrpToken"
•	POST  /api/users/login → returns a token "{username}-mrpToken"
•	POST  /api/media → stub (returns 204)
•	GET   /api/media/{id} → stub (returns 204)
•	PUT   /api/media/{id} → stub (returns 204)
•	DELETE /api/media/{id} → stub (returns 204)

⸻

2) Layered architecture

Dependency direction:
Presentation → Application → Domain and Infrastructure → Application/Domain.

⸻

3) Current folder structure (high-level)

/Domain
(Entities only for now)

/Application
/Repositories
IUserRepository.cs
/Services
ITokenService.cs
/UseCases/Users
RegisterUserCommand.cs
RegisterUserResult.cs
RegisterUserHandler.cs
LoginUserCommand.cs
LoginUserResult.cs
LoginUserHandler.cs

/Infrastructure
/Security
StaticTokenService.cs     // returns "{username}-mrpToken"

/Presentation
/Http
HttpRequest.cs
HttpResponse.cs
/Routing
/Attributes
RouteAttribute.cs
HttpMethodAttribute.cs
HttpGetAttribute.cs
HttpPostAttribute.cs
HttpPutAttribute.cs
HttpDeleteAttribute.cs
PathTemplate.cs
RouteDefinition.cs
Router.cs
/Server
Server.cs
/Contracts/Auth
RegisterRequest.cs
LoginRequest.cs
TokenResponse.cs
/Controllers
UsersController.cs        // [Route("api/users")]
MediaController.cs        // [Route("api/media")]
App.cs                      // tiny manual “DI” bootstrap
Program.cs                  // composition root (startup)

One class per file: I follow this consistently.

⸻

4) Routing approach

I built a minimal server on HttpListener and a small router that:
•	Scans assemblies for controllers with [Route("...")] on the class and [HttpGet|HttpPost|HttpPut|HttpDelete("...")] on methods.
•	Supports simple path templates with {id} parameters.
•	Invokes controller methods with (HttpRequest, HttpResponse); if nothing is written, HttpResponse defaults to 204 No Content.


⸻

5) Use-cases & Repositories (Application layer)

I created explicit use-cases for auth:
•	RegisterUserHandler : HandleAsync(RegisterUserCommand) → RegisterUserResult
•	LoginUserHandler : HandleAsync(LoginUserCommand) → LoginUserResult

And ITokenService and IUserRepository (interfaces) for future expansion:
•	ITokenService — generate tokens
•	IUserRepository — defined but not used yet (no storage yet)

Right now the handlers just validate inputs and ask ITokenService to generate the token.

⸻

6) Infrastructure (stub implementation)

For the token, I added a tiny implementation:
•	StaticTokenService : ITokenService → returns "{username}-mrpToken"

⸻

7) Composition & wiring (startup)

I use a tiny bootstrap class to keep controllers dumb and keep Application available:
•	App.Configure() instantiates StaticTokenService, then the handlers:
•	RegisterUserHandler(tokenService)
•	LoginUserHandler(tokenService)

Program.cs calls App.Configure(), builds the Router (assembly scanning), starts HttpServer on http://localhost:8080/.

⸻

8) Controllers (Presentation layer)

UsersController
•	[Route("api/users")]
•	POST /api/users/register
Parses RegisterRequest, validates, calls RegisterUserHandler, returns TokenResponse.
•	POST /api/users/login
Parses LoginRequest, validates, calls LoginUserHandler, returns TokenResponse.

Validation failures return 400 with empty body (simple for now).

MediaController (stubs)
•	[Route("api/media")]
•	POST /api/media
•	GET /api/media/{id}
•	PUT /api/media/{id}
•	DELETE /api/media/{id}
All are currently stubs and therefore return 204 (no content) by design.

⸻

9) How to run
    Build & run:

# Register
curl -i -X POST http://localhost:8080/api/users/register \
-H "Content-Type: application/json" \
-d '{"username":"mahdi","password":"123"}'
# → 200 OK
# → {"token":"mahdi-mrpToken"}

# Login
curl -i -X POST http://localhost:8080/api/users/login \
-H "Content-Type: application/json" \
-d '{"username":"mahdi","password":"123"}'
# → 200 OK
# → {"token":"mahdi-mrpToken"}

# Media stubs (all 204 No Content)
curl -i -X POST   http://localhost:8080/api/media
curl -i -X GET    http://localhost:8080/api/media/1
curl -i -X PUT    http://localhost:8080/api/media/1
curl -i -X DELETE http://localhost:8080/api/media/1


Github url: https://github.com/Mahdi-IvI/csharp-server.git
