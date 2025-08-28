namespace API.DTO;

public class UserDTO
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string Role { get; set; } = null!;
}