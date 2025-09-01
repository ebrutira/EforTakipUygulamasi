using System.ComponentModel.DataAnnotations;

namespace EforTakipUygulamasi.Models
{
    public class FileAttachment
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string OriginalName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public string UploadedBy { get; set; } = string.Empty;

        public Request? Request { get; set; }
    }
}