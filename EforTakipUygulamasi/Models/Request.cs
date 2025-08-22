using System.ComponentModel.DataAnnotations;

namespace EforTakipUygulamasi.Models
{
    public class Request
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Talep Adı zorunludur")]
        [StringLength(200, ErrorMessage = "Talep Adı en fazla 200 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.New;

        public RequestPriority Priority { get; set; } = RequestPriority.Medium;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? Deadline { get; set; }

        public DateTime? KKTDeadline { get; set; }

        // Efor Saatleri
        public decimal AnalystHours { get; set; } = 0;
        public decimal DeveloperHours { get; set; } = 0;
        public decimal KKTHours { get; set; } = 0;
        public decimal PreprodHours { get; set; } = 0;

        // Hesaplanan Alanlar
        public decimal TotalHours => AnalystHours + DeveloperHours + KKTHours + PreprodHours;
        public decimal TotalManDays => TotalHours / 8; // 8 saat = 1 adam-gün

        public TShirtSize Size
        {
            get
            {
                var manDays = TotalManDays;
                return manDays switch
                {
                    <= 5 => TShirtSize.FastTrack,
                    <= 10 => TShirtSize.XS,
                    <= 25 => TShirtSize.S,
                    <= 50 => TShirtSize.M,
                    <= 100 => TShirtSize.L,
                    _ => TShirtSize.XL
                };
            }
        }

        public int StoryPoints
        {
            get
            {
                var hours = TotalHours;
                return hours switch
                {
                    <= 16 => 3,
                    <= 24 => 5,
                    <= 30 => 8,
                    <= 40 => 13,
                    _ => 21
                };
            }
        }

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime LastModified { get; set; } = DateTime.Now;
        public string LastModifiedBy { get; set; } = string.Empty;
    }

    public enum RequestStatus
    {
        New = 1,
        InProgress = 2,
        Testing = 3,
        Completed = 4,
        Cancelled = 5
    }

    public enum RequestPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum TShirtSize
    {
        FastTrack,
        XS,
        S,
        M,
        L,
        XL
    }
}