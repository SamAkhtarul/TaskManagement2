using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Models;

public class Employee
{
    public int Id { get; set; }
    [StringLength(25, ErrorMessage ="Name must be less than or equal 25 characters.")]
    public string Name { get; set; } = string.Empty;
    [StringLength(11, ErrorMessage = "Phone no must be 11 characters.")]
    public string PhoneNo { get; set; } = string.Empty;
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; } = string.Empty;
    
    public string? PicturePath { get; set; }
    [NotMapped]
    public IFormFile? Picture { get; set; }
    
}
