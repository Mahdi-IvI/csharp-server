using NSubstitute;
using semester_project._2Application.Interfaces;
using semester_project._2Application.UseCases.Users;
using semester_project._3Domain.Entities;

namespace Tests.Users;

[TestFixture]
public class GetUserByUsernameHandlerTests
{
    [Test]
    public void HandleAsync_WhenUserNotFound_ThrowsKeyNotFound()
    {
        var users = Substitute.For<IUserRepository>();
        users.GetByUsernameAsync("mahdi").Returns((User?)null);

        var sut = new GetUserByUsernameHandler(users);

        Assert.ThrowsAsync<KeyNotFoundException>(() => sut.HandleAsync("mahdi"));
    }

    [Test]
    public async Task HandleAsync_WhenUserExists_ReturnsUser()
    {
        var users = Substitute.For<IUserRepository>();
        var u = new User(
            Id: 1,
            Username: "mahdi",
            Password: "pass",
            FirstName: null,
            LastName: null,
            FavoriteGenre: null,
            CreatedAt: DateTime.UtcNow
        );

        users.GetByUsernameAsync("mahdi").Returns(u);

        var sut = new GetUserByUsernameHandler(users);

        var result = await sut.HandleAsync("mahdi");

        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Username, Is.EqualTo("mahdi"));
    }
}