
namespace semester_project.Domain.Entities;

public class User(string id, string username, string password)
{
    public string Id { get; set; } = id;
    public string UserName { get; set; } = username;
    public string Password { get; set; } = password;
    
}