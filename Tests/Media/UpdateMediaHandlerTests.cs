using NSubstitute;
using semester_project._2Application.Interfaces;
using semester_project._2Application.UseCases.Media;
using semester_project._3Domain.Enums;

namespace Tests.Media;

[TestFixture]
public class UpdateMediaHandlerTests
{
    [Test]
    public void HandleAsync_WhenTitleMissing_Throws()
    {
        var repo = Substitute.For<IMediaRepository>();
        var sut = new UpdateMediaHandler(repo);

        var input = new UpdateMediaInput(
            CreatedByUserId: 1,
            MediaId: 10,
            Title: "",
            Description: "x",
            MediaType: "movie",
            ReleaseYear: 2010,
            Genres: Array.Empty<string>(),
            AgeRestriction: 12
        );

        Assert.ThrowsAsync<ArgumentException>(() => sut.HandleAsync(input));
    }

    [Test]
    public async Task HandleAsync_WhenMediaNotFound_ThrowsKeyNotFound()
    {
        var repo = Substitute.For<IMediaRepository>();
        repo.GetCreatorIdAsync(10).Returns((long?)null);

        var sut = new UpdateMediaHandler(repo);

        var input = new UpdateMediaInput(
            CreatedByUserId: 1,
            MediaId: 10,
            Title: "T",
            Description: "x",
            MediaType: "movie",
            ReleaseYear: 2010,
            Genres: Array.Empty<string>(),
            AgeRestriction: 12
        );

        Assert.ThrowsAsync<KeyNotFoundException>(() => sut.HandleAsync(input));
        await repo.DidNotReceive().UpdateAsync(Arg.Any<semester_project._3Domain.Entities.Media>());
    }

    [Test]
    public async Task HandleAsync_WhenRequesterNotCreator_ThrowsForbidden()
    {
        var repo = Substitute.For<IMediaRepository>();
        repo.GetCreatorIdAsync(10).Returns(999);

        var sut = new UpdateMediaHandler(repo);

        var input = new UpdateMediaInput(
            CreatedByUserId: 1, // not the creator
            MediaId: 10,
            Title: "T",
            Description: "x",
            MediaType: "movie",
            ReleaseYear: 2010,
            Genres: Array.Empty<string>(),
            AgeRestriction: 12
        );

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync(input));
        await repo.DidNotReceive().UpdateAsync(Arg.Any<semester_project._3Domain.Entities.Media>());
    }

    [Test]
    public async Task HandleAsync_WhenValid_UpdatesMedia()
    {
        var repo = Substitute.For<IMediaRepository>();
        repo.GetCreatorIdAsync(10).Returns(1);
        repo.UpdateAsync(Arg.Any<semester_project._3Domain.Entities.Media>()).Returns(true);

        var sut = new UpdateMediaHandler(repo);

        var someGenreName = Enum.GetNames(typeof(Genre)).FirstOrDefault() ?? "action";

        var input = new UpdateMediaInput(
            CreatedByUserId: 1,
            MediaId: 10,
            Title: "New Title",
            Description: null, // should become ""
            MediaType: "movie",
            ReleaseYear: null,
            Genres: new[] { someGenreName, "not-a-genre", someGenreName },
            AgeRestriction: 18
        );

        await sut.HandleAsync(input);

        await repo.Received(1).UpdateAsync(Arg.Is<semester_project._3Domain.Entities.Media>(m =>
            m.Id == 10 &&
            m.Title == "New Title" &&
            m.Description == "" &&
            m.CreatedBy == 1 &&
            m.ReleaseYear == "2025" &&
            m.Genres.Count == 1
        ));
    }
}