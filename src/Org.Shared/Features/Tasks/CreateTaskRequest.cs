namespace Org.Shared.Features.Tasks
{
    public class CreateTaskRequest
    {
        public string? Title { get; set; }
        public System.DateTime? DueDate { get; set; } = System.DateTime.Today.AddDays(1);
    }
}