using System.ComponentModel.DataAnnotations;
using EforTakipUygulamasi.Common;

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
        public RequestStatusEnum Status { get; set; } = RequestStatusEnum.New;

        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? KKTDeadline { get; set; }

        public decimal AnalystHours { get; set; } = 0;        
        public decimal DeveloperHours { get; set; } = 0;     
        public decimal KKTHours { get; set; } = 0;         

        public decimal PreprodHours
        {
            get => 0; 
            set { } 
        }

        public decimal TotalHours => AnalystHours + DeveloperHours + KKTHours;
        public decimal TotalManDays => TotalHours / 8;

        public TShirtSizeEnum Size
        {
            get
            {
                var manDays = TotalManDays;
                return manDays switch
                {
                    <= 5 => TShirtSizeEnum.FastTrack,
                    <= 10 => TShirtSizeEnum.XS,
                    <= 25 => TShirtSizeEnum.S,
                    <= 50 => TShirtSizeEnum.M,
                    <= 100 => TShirtSizeEnum.L,
                    _ => TShirtSizeEnum.XL
                };
            }
        }


        public string Assignee { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime LastModified { get; set; } = DateTime.Now;
        public string LastModifiedBy { get; set; } = string.Empty;

        public bool IsOverdue => Deadline.HasValue && Deadline.Value < DateTime.Now;
    }
}