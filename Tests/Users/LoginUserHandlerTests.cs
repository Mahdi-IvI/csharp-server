using NSubstitute;
using semester_project._2Application.Interfaces;
using semester_project._2Application.UseCases.Users;
using semester_project._3Domain.Entities;

namespace Tests.Users;

[TestFixture]
public class LoginUserHandlerTests
{
    [Test]
    public void Handle_WhenUsernameMissing_ThrowsArgumentException()
    {
        var users = Substitute.For<IUserRepository>();
        var tokens = Substitute.For<ITokenService>();
        var sut = new LoginUserHandler(users, tokens);

        Assert.ThrowsAsync<ArgumentException>(() =>
            sut.Handle(new LoginUserInput("   ", "pass")));
    }

    [Test]
    public void Handle_WhenPasswordMissing_ThrowsArgumentException()
    {
        var users = Substitute.For<IUserRepository>();
        var tokens = Substitute.For<ITokenService>();
        var sut = new LoginUserHandler(users, tokens);

        Assert.ThrowsAsync<ArgumentException>(() =>
            sut.Handle(new LoginUserInput("mahdi", "   ")));
    }

    [Test]
    public async Task Handle_WhenValidCredentials_ReturnsGeneratedToken()
    {
        var users = Substitute.For<IUserRepository>();
        var tokens = Substitute.For<ITokenService>();

        users.GetByUsernameAsync("mahdi").Returns(new User(
            Id: 1,
            Username: "mahdi",
            Password: "pass",
            FirstName: null,
            LastName: null,
            FavoriteGenre: null,
            CreatedAt: DateTime.UtcNow
        ));

        tokens.Generate("mahdi").Returns("tok_abc");

        var sut = new LoginUserHandler(users, tokens);

        var result = await sut.Handle(new LoginUserInput("mahdi", "pass"));

        Assert.That(result.Token, Is.EqualTo("tok_abc"));
        tokens.Received(1).Generate("mahdi");
    }

    [Test]
    public async Task Handle_WhenUserNotFound_ReturnsFallbackTokenToken()
    {
        var users = Substitute.For<IUserRepository>();
        var tokens = Substitute.For<ITokenService>();
        users.GetByUsernameAsync("mahdi").Returns((User?)null);

        var sut = new LoginUserHandler(users, tokens);

        var result = await sut.Handle(new LoginUserInput("mahdi", "pass"));

        Assert.That(result.Token, Is.EqualTo("token"));
        tokens.DidNotReceiveWithAnyArgs().Generate(default!);
    }

    [Test]
    public async Task Handle_WhenPasswordWrong_ReturnsFallbackTokenToken()
    {
        var users = Substitute.For<IUserRepository>();
        var tokens = Substitute.For<ITokenService>();

        users.GetByUsernameAsync("mahdi").Returns(new User(
            Id: 1,
            Username: "mahdi",
            Password: "correct",
            FirstName: null,
            LastName: null,
            FavoriteGenre: null,
            CreatedAt: DateTime.UtcNow
        ));

        var sut = new LoginUserHandler(users, tokens);

        var result = await sut.Handle(new LoginUserInput("mahdi", "wrong"));

        Assert.That(result.Token, Is.EqualTo("token"));
        tokens.DidNotReceiveWithAnyArgs().Generate(default!);
    }
}