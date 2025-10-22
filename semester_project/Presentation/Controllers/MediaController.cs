using semester_project.Presentation.Http;
using semester_project.Presentation.Http.Routing.Attributes;

namespace semester_project.Presentation.Controllers;

[Route("media")]
public sealed class MediaController
{
    // POST /media/
    [HttpPost]
    public Task Create(HttpRequest req, HttpResponse res)
    {
        // Empty 204 response (default)
        return Task.CompletedTask;
    }
    
    // DELETE /media/{id}
    [HttpDelete("{id}")]
    public Task Delete(HttpRequest req, HttpResponse res)
    {
        return Task.CompletedTask;
    }

    // PUT /media/{id}
    [HttpPut("{id}")]
    public Task Update(HttpRequest req, HttpResponse res)
    {
        return Task.CompletedTask;
    }

    // GET /media/{id}
    [HttpGet("{id}")]
    public Task GetById(HttpRequest req, HttpResponse res)
    {
        return Task.CompletedTask;
    }
}