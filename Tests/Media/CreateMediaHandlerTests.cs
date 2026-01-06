using NSubstitute;
using semester_project._2Application.Interfaces;
using semester_project._2Application.UseCases.Media;
using semester_project._3Domain.Enums;

namespace Tests.Media;


[TestFixture]
public class CreateMediaHandlerTests
{
    [Test]
    public void HandleAsync_WhenTitleMissing_Throws()
    {
        var repo = Substitute.For<IMediaRepository>();
        var sut = new CreateMediaHandler(repo);

        var input = new CreateMediaInput(
            CreatedByUserId: 1,
            Title: "   ",
            Description: "desc",
            MediaType: "movie",
            ReleaseYear: 2010,
            Genres: Array.Empty<string>(),
            AgeRestriction: 12
        );

        Assert.ThrowsAsync<ArgumentException>(() => sut.HandleAsync(input));
    }

    [Test]
    public void HandleAsync_WhenMediaTypeNotAllowed_Throws()
    {
        var repo = Substitute.For<IMediaRepository>();
        var sut = new CreateMediaHandler(repo);

        var input = new CreateMediaInput(
            CreatedByUserId: 1,
            Title: "Inception",
            Description: "desc",
            MediaType: "documentary", // not in allowed set
            ReleaseYear: 2010,
            Genres: Array.Empty<string>(),
            AgeRestriction: 12
        );

        Assert.ThrowsAsync<ArgumentException>(() => sut.HandleAsync(input));
    }

    [Test]
    public async Task HandleAsync_WhenValid_CallsAddAsync_AndReturnsNewId()
    {
        var repo = Substitute.For<IMediaRepository>();
        repo.AddAsync(Arg.Any<semester_project._3Domain.Entities.Media>()).Returns(123);

        var sut = new CreateMediaHandler(repo);

        // pick a *real* Genre name from your enum so the handler parses it successfully
        var someGenreName = Enum.GetNames(typeof(Genre)).FirstOrDefault() ?? "action";

        var input = new CreateMediaInput(
            CreatedByUserId: 77, 
            Title: "Inception",
            Description: null,        // should become ""
            MediaType: "movie",
            ReleaseYear: null,        // handler defaults to 2025
            Genres: new[] { someGenreName, someGenreName, "   ", "not-a-genre" },
            AgeRestriction: 16
        );

        var result = await sut.HandleAsync(input);

        Assert.That(result.Id, Is.EqualTo(123));

        await repo.Received(1).AddAsync(Arg.Is<semester_project._3Domain.Entities.Media>(m =>
            m.Id == 0 &&
            m.Title == "Inception" &&
            m.Description == "" &&
            m.CreatedBy == 77 &&
            m.AgeRestriction == 16 &&
            m.ReleaseYear == "2025" &&                  // default applied
            m.Genres.Count == 1                          // duplicates removed; invalid ignored
        ));
    }
}