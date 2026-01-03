using semester_project._2Application.Interfaces;
using semester_project._3Domain.Enums;

namespace semester_project._2Application.UseCases.Users;

public class UpdateUserProfileHandler(IUserRepository users)
{
    public async Task HandleAsync(UpdateUserProfileInput input)
    {
        // Require at least one field to change
        var hasAny = !(string.IsNullOrWhiteSpace(input.Email)
                       && string.IsNullOrWhiteSpace(input.FirstName)
                       && string.IsNullOrWhiteSpace(input.LastName)
                       && string.IsNullOrWhiteSpace(input.FavoriteGenre));
        if (!hasAny) throw new ArgumentException("No changes provided.");

        Genre? parsed = null;
        if (!string.IsNullOrWhiteSpace(input.FavoriteGenre))
        {
            if (!Enum.TryParse<Genre>(input.FavoriteGenre, ignoreCase: true, out var g))
                throw new ArgumentException("Invalid favoriteGenre value.");
            parsed = g;
        }

        var updated = await users.UpdateProfileAsync(
            input.UserId, input.Email, input.FirstName, input.LastName, parsed
        ).ConfigureAwait(false);

        if (!updated) throw new KeyNotFoundException("User not found.");
    }
}