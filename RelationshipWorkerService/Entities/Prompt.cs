using System;
using System.Collections.Generic;
using System.Text;

namespace RelationshipWorkerService.Entities
{
    public class Prompt
    {
        public int Id { get; set; }
        public string? PromptText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsComplete { get; set; } = false;
    }
}