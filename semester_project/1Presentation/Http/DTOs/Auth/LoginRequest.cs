namespace semester_project._1Presentation.Http.Contracts.Auth;

public record LoginRequest(
    string? Username,
    string? Password
);