using System.Text.Json;
using semester_project.Application.Common;
using semester_project.Application.UseCases.Users;
using semester_project.Presentation.Http;
using semester_project.Presentation.Http.Contracts;
using semester_project.Presentation.Http.Contracts.Auth;
using semester_project.Presentation.Http.Routing.Attributes;

namespace semester_project.Presentation.Controllers;

[Route("users")]
public sealed class UsersController
{
    private static readonly JsonSerializerOptions Json =
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true, AllowTrailingCommas = true, };

    // POST /auth/register
    [HttpPost("register")]
    public async Task Register(HttpRequest req, HttpResponse res)
    {
        var body = await req.ReadBodyAsStringAsync().ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<RegisterRequest>(body, Json);

        if (string.IsNullOrWhiteSpace(dto?.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            await res.WriteJsonAsync(new ApiError("BadRequest", "Username and password are required."), 400);
            return;
        }

        try
        {
            var result = await App.RegisterUser.Handle(
                new RegisterUserInput(dto.Username!, dto.Password!)
            );
            await res.WriteJsonAsync(new TokenResponse(result.Token), 200);
        }
        catch (UsernameAlreadyExistsException ex)
        {
            // 409: Conflict
            await res.WriteJsonAsync(new ApiError("UserExists", ex.Message), 409);
        }
    }

    // POST /auth/login
    [HttpPost("login")]
    public async Task Login(HttpRequest req, HttpResponse res)
    {
        var body = await req.ReadBodyAsStringAsync().ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<LoginRequest>(body, Json);

        if (string.IsNullOrWhiteSpace(dto?.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            await res.WriteJsonAsync(new ApiError("BadRequest", "Username and password are required."), 400);
            return;
        }

        try
        {
            var result = await App.LoginUser.Handle(
                new LoginUserInput(dto.Username!, dto.Password!)
            );
            await res.WriteJsonAsync(new TokenResponse(result.Token), 200);
        }
        catch (InvalidCredentialsException ex)
        {
            await res.WriteJsonAsync(new ApiError("InvalidCredentials", ex.Message), 401);
        }
    }
}