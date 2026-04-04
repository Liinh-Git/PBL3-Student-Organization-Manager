using System;

namespace Org.Shared.Features.Tasks
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; } // "TODO", "IN_PROGRESS", "DONE"
        public string? AssigneeName { get; set; }
        public DateTime? DueDate { get; set; }
    }
}