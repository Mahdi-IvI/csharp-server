namespace semester_project._1Presentation.Http.Contracts.Auth;

public record RegisterRequest(
    string? Password,
    string? Username
);