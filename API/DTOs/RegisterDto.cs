using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    public required string Gender { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string KnownAs { get; set; }

    [Required]
    public required DateOnly DateOfBirth { get; set; }

    [Required]
    public required string City { get; set; }

    [Required]
    public required string Country { get; set; }

    [Required]
    [MinLength(8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$", 
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one number, and one special character.")]
    public required string Password { get; set; }

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public required string ConfirmPassword { get; set; }
}
