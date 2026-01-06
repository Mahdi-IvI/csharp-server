using NSubstitute;
using semester_project._2Application.Interfaces;
using semester_project._2Application.UseCases.Media;

namespace Tests.Media;

[TestFixture]
public class DeleteMediaHandlerTests
{
    [Test]
    public async Task HandleAsync_WhenMediaNotFoundInitially_ThrowsKeyNotFound()
    {
        var repo = Substitute.For<IMediaRepository>();
        repo.GetCreatorIdAsync(10).Returns((long?)null);

        var sut = new DeleteMediaHandler(repo);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.HandleAsync(new DeleteMediaInput(MediaId: 10, RequesterUserId: 1)));

        Assert.That(ex!.Message, Is.EqualTo("Media not found."));
        await repo.DidNotReceive().DeleteAsync(Arg.Any<long>());
    }

    [Test]
    public async Task HandleAsync_WhenNotCreator_ThrowsForbidden()
    {
        var repo = Substitute.For<IMediaRepository>();
        repo.GetCreatorIdAsync(10).Returns(99);

        var sut = new DeleteMediaHandler(repo);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.HandleAsync(new DeleteMediaInput(MediaId: 10, RequesterUserId: 1)));

        await repo.DidNotReceive().DeleteAsync(Arg.Any<long>());
    }

    [Test]
    public async Task HandleAsync_WhenDeleteReturnsFalse_ThrowsKeyNotFound()
    {
        var repo = Substitute.For<IMediaRepository>();
        repo.GetCreatorIdAsync(10).Returns(1);
        repo.DeleteAsync(10).Returns(false);

        var sut = new DeleteMediaHandler(repo);

        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.HandleAsync(new DeleteMediaInput(MediaId: 10, RequesterUserId: 1)));
    }

    [Test]
    public async Task HandleAsync_WhenValid_Deletes()
    {
        var repo = Substitute.For<IMediaRepository>();
        repo.GetCreatorIdAsync(10).Returns(1);
        repo.DeleteAsync(10).Returns(true);

        var sut = new DeleteMediaHandler(repo);

        Assert.DoesNotThrowAsync(() =>
            sut.HandleAsync(new DeleteMediaInput(MediaId: 10, RequesterUserId: 1)));

        await repo.Received(1).DeleteAsync(10);
    }
}