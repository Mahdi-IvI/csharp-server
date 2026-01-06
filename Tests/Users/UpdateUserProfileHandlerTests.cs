using NSubstitute;
using semester_project._2Application.Interfaces;
using semester_project._2Application.UseCases.Users;
using semester_project._3Domain.Enums;

namespace Tests.Users;

[TestFixture]
public class UpdateUserProfileHandlerTests
{
    [Test]
    public void HandleAsync_WhenNoChangesProvided_ThrowsArgumentException()
    {
        var users = Substitute.For<IUserRepository>();
        var sut = new UpdateUserProfileHandler(users);

        var input = new UpdateUserProfileInput(
            UserId: 1,
            Email: null,
            FirstName: null,
            LastName: null,
            FavoriteGenre: null
        );

        Assert.ThrowsAsync<ArgumentException>(() => sut.HandleAsync(input));
    }

    [Test]
    public void HandleAsync_WhenFavoriteGenreInvalid_ThrowsArgumentException()
    {
        var users = Substitute.For<IUserRepository>();
        var sut = new UpdateUserProfileHandler(users);

        var input = new UpdateUserProfileInput(
            UserId: 1,
            Email: null,
            FirstName: "Mahdi",
            LastName: null,
            FavoriteGenre: "not-a-genre"
        );

        Assert.ThrowsAsync<ArgumentException>(() => sut.HandleAsync(input));
    }

    [Test]
    public void HandleAsync_WhenRepoReturnsFalse_ThrowsKeyNotFound()
    {
        var users = Substitute.For<IUserRepository>();
        users.UpdateProfileAsync(1, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<Genre?>())
            .Returns(false);

        var sut = new UpdateUserProfileHandler(users);

        var input = new UpdateUserProfileInput(
            UserId: 1,
            Email: "a@b.com",
            FirstName: null,
            LastName: null,
            FavoriteGenre: null
        );

        Assert.ThrowsAsync<KeyNotFoundException>(() => sut.HandleAsync(input));
    }

    [Test]
    public async Task HandleAsync_WhenValid_ParsesGenre_AndCallsRepository()
    {
        var users = Substitute.For<IUserRepository>();
        users.UpdateProfileAsync(Arg.Any<long>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<Genre?>())
            .Returns(true);

        var sut = new UpdateUserProfileHandler(users);

        var genreName = Enum.GetNames(typeof(Genre)).First();
        var input = new UpdateUserProfileInput(
            UserId: 5,
            Email: "mahdi@x.com",
            FirstName: "Mahdi",
            LastName: "Abbasi",
            FavoriteGenre: genreName
        );

        await sut.HandleAsync(input);

        await users.Received(1).UpdateProfileAsync(
            5,
            "mahdi@x.com",
            "Mahdi",
            "Abbasi",
            Arg.Is<Genre?>(g => g.HasValue && g.Value.ToString().Equals(genreName, StringComparison.OrdinalIgnoreCase))
        );
    }
}