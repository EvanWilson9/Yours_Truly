using RelationshipWorkerService.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RelationshipWorkerService.Entities
{
    public class PromptResponse
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int PromptId { get; set; }
        public Prompt Prompt { get; set; } = null!;
        public EmailStatus Status { get; set; } = EmailStatus.Pending;
        public string? ResponseText { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResponseAt { get; set; }
    }
}