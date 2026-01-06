using NSubstitute;
using semester_project._2Application.Interfaces;
using semester_project._2Application.UseCases.Users;
using semester_project._3Domain.Entities;

namespace Tests.Users;

[TestFixture]
public class GetUserProfileHandlerTests
{
    [Test]
    public void HandleAsync_WhenUserNotFound_ThrowsKeyNotFound()
    {
        var users = Substitute.For<IUserRepository>();
        users.GetByIdAsync(10).Returns((User?)null);

        var sut = new GetUserProfileHandler(users);

        Assert.ThrowsAsync<KeyNotFoundException>(() => sut.HandleAsync(10));
    }

    [Test]
    public async Task HandleAsync_WhenUserExists_ReturnsUser()
    {
        var users = Substitute.For<IUserRepository>();
        var u = new User(
            Id: 10,
            Username: "mahdi",
            Password: "pass",
            FirstName: "Mahdi",
            LastName: "Abbasi",
            FavoriteGenre: null,
            CreatedAt: DateTime.UtcNow
        );

        users.GetByIdAsync(10).Returns(u);

        var sut = new GetUserProfileHandler(users);

        var result = await sut.HandleAsync(10);

        Assert.That(result.Id, Is.EqualTo(10));
        Assert.That(result.Username, Is.EqualTo("mahdi"));
    }
}