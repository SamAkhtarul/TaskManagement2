using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Models;
public enum TaskStatus
{
    Pending=1,
    InProgress,
    Completed,
    Overdue
}
public class AssignedTask
{
    //Scalar Properties
    public int Id { get; set; }
    [ForeignKey("Task")]
    public int TaskId { get; set; }
    [ForeignKey("User")]
    public int UserId { get; set; }
    [NotMapped]
    public List<Employee> Users { get; set; } = new List<Employee>();
    [NotMapped]
    public List<TaskList> Tasklist { get; set; } = new List<TaskList>();
    [DataType(DataType.Date)]
    public DateTime AssignedDate { get; set; }
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }
    [DataType(DataType.Date)]
    public DateTime SubmitDate { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    [StringLength(50, ErrorMessage = "Remarks must be less than or equal to 50 characters.")]
    public string? Remarks { get; set; }
    //Navigation properties
    [ValidateNever]
    public TaskList Task { get; set; } = null!;
    [ValidateNever]
    public Employee User { get; set; } = null!;


}
