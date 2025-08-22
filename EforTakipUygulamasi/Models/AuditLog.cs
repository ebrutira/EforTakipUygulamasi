namespace EforTakipUygulamasi.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Entity { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Field { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE
    }
}