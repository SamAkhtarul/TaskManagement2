using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Models;

namespace TaskManagement.Data;

public class TaskDbContext:IdentityDbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options): base(options)
    {

    }
    public DbSet<TaskList> Tasks { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<AssignedTask> AssignedTasks { get; set; }

}
