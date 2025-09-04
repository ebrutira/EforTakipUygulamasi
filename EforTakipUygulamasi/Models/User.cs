using System.ComponentModel.DataAnnotations;
using EforTakipUygulamasi.Common;

namespace EforTakipUygulamasi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        // Hash'lenmiş şifre - plain text asla saklanmaz
        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Viewer;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastLogin { get; set; }

        // Güvenlik alanları
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LastFailedLogin { get; set; }
        public bool IsLocked { get; set; } = false;
        public DateTime? LockoutEnd { get; set; }

        // Helper properties
        public bool IsAccountLocked => IsLocked && LockoutEnd > DateTime.Now;

        public string RoleDisplayName
        {
            get
            {
                return Role switch
                {
                    UserRole.Admin => "Yönetici",
                    UserRole.Developer => "Geliştirici",
                    UserRole.Viewer => "Görüntüleyici",
                    _ => "Tanımsız"
                };
            }
        }
    }
}