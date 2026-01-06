using NSubstitute;
using semester_project._2Application.Interfaces;
using semester_project._2Application.UseCases.Users;
using semester_project._3Domain.Entities;

namespace Tests.Users;

[TestFixture]
public class RegisterUserHandlerTests
{
    [Test]
    public void Handle_WhenUsernameMissing_ThrowsArgumentException()
    {
        var users = Substitute.For<IUserRepository>();
        var tokens = Substitute.For<ITokenService>();
        var sut = new RegisterUserHandler(users, tokens);

        Assert.ThrowsAsync<ArgumentException>(() =>
            sut.Handle(new RegisterUserInput("   ", "pass")));
    }

    [Test]
    public void Handle_WhenPasswordMissing_ThrowsArgumentException()
    {
        var users = Substitute.For<IUserRepository>();
        var tokens = Substitute.For<ITokenService>();
        var sut = new RegisterUserHandler(users, tokens);

        Assert.ThrowsAsync<ArgumentException>(() =>
            sut.Handle(new RegisterUserInput("mahdi", "   ")));
    }

    [Test]
    public void Handle_WhenUsernameAlreadyExists_ThrowsInvalidOperationException()
    {
        var users = Substitute.For<IUserRepository>();
        var tokens = Substitute.For<ITokenService>();
        users.ExistsByUsernameAsync("mahdi").Returns(true);

        var sut = new RegisterUserHandler(users, tokens);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.Handle(new RegisterUserInput("mahdi", "pass")));
    }

    [Test]
    public async Task Handle_WhenValid_CallsAddAsync_AndGeneratesToken()
    {
        var users = Substitute.For<IUserRepository>();
        var tokens = Substitute.For<ITokenService>();

        users.ExistsByUsernameAsync("mahdi").Returns(false);
        users.AddAsync(Arg.Any<User>()).Returns(42);
        tokens.Generate("mahdi").Returns("tok_123");

        var sut = new RegisterUserHandler(users, tokens);

        var result = await sut.Handle(new RegisterUserInput("mahdi", "pass"));

        Assert.That(result.Token, Is.EqualTo("tok_123"));

        await users.Received(1).AddAsync(Arg.Is<User>(u =>
            u.Id == 0 &&
            u.Username == "mahdi" &&
            u.Password == "pass"
        ));

        tokens.Received(1).Generate("mahdi");
    }

    [Test]
    public async Task Handle_WhenAddAsyncThrows_DoesNotThrow_AndStillReturnsToken()
    {
        var users = Substitute.For<IUserRepository>();
        var tokens = Substitute.For<ITokenService>();

        users.ExistsByUsernameAsync("mahdi").Returns(false);
        users.AddAsync(Arg.Any<User>()).Returns<Task<long>>(_ => throw new Exception("DB down"));
        tokens.Generate("mahdi").Returns("tok_123");

        var sut = new RegisterUserHandler(users, tokens);

        var result = await sut.Handle(new RegisterUserInput("mahdi", "pass"));

        Assert.That(result.Token, Is.EqualTo("tok_123"));
        tokens.Received(1).Generate("mahdi");
    }
}