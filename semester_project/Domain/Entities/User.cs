namespace semester_project.models;

[ApiController]
[Route("users")]
public class User(string id, string username, string password)
{
    public string Id { get; set; } = id;
    public string UserName { get; set; } = username;
    public string Password { get; set; } = password;
    
}