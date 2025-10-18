using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models;
public enum TaskPriority
{
    Low,
    Medium,
    High
}

public class TaskList
{
    //scalar Property
    public int Id { get; set; }
    [StringLength(50, ErrorMessage = "Title must be less than or equal to 50 characters.")]

    public string Title { get; set; } = string.Empty;
    [StringLength(200, ErrorMessage = "Description must be less than or equal to 200 characters.")]
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }

    //Navigation property
    public ICollection<AssignedTask> AssignedTasks { get; set; } = new List<AssignedTask>();


}
