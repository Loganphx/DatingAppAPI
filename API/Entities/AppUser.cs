using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using API.Extensions;

namespace API.Entities;

public class AppUser
{
    public int Id { get; set; }
    
    [Required]
    public string Username { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [JsonIgnore]
    public byte[] PasswordHash { get; set; }
    
    [Required]
    [JsonIgnore]
    public byte[] PasswordSalt { get; set; }
    
    [JsonIgnore]
    public DateOnly DateOfBirth { get; set; }

    public string KnownAs { get; set; } = "";

    public DateTime    Created      { get; set; } = DateTime.UtcNow;
    public DateTime    LastActive   { get; set; } = DateTime.UtcNow;
    public string      Gender       { get; set; } = "";
    public string      Introduction { get; set; } = "";
    public string      LookingFor   { get; set; } = "";
    public string      Interests    { get; set; } = "";
    public string      City         { get; set; } = "";
    public string      Country      { get; set; } = "";
    public List<Photo> Photos       { get; set; } = new();

    public int CalculateAge()
    {
        return DateOfBirth.CalculateAge();
    }
}
