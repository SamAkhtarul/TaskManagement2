
using TaskManagement.Models;

namespace TaskManagement.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public List<AssignedTask> RecentTasks { get; set; } = new List<AssignedTask>();
    }
}

