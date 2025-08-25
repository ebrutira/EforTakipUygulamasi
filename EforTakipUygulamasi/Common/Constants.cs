namespace EforTakipUygulamasi.Common
{
    public static class Constants
    {
        public static class Status
        {
            public const string New = "Yeni";
            public const string InProgress = "Devam Ediyor";
            public const string Completed = "Tamamlandı";
            public const string OnHold = "Beklemede";
            public const string Cancelled = "İptal";
        }

        public static class Priority
        {
            public const string Low = "Düşük";
            public const string Medium = "Orta";
            public const string High = "Yüksek";
            public const string Critical = "Kritik";
        }

        public static class Colors
        {
            public const string Green = "#28a745";    // Fast Track, XS
            public const string Yellow = "#ffc107";   // S
            public const string Orange = "#fd7e14";   // M
            public const string Red = "#dc3545";      // L
            public const string Purple = "#6f42c1";   // XL
        }

        public static class UserRoles
        {
            public const string Admin = "Admin";
            public const string Contributor = "Contributor";
            public const string Viewer = "Viewer";
        }

        public static class FileSettings
        {
            public const string UploadPath = "wwwroot/uploads";
            public const int MaxFileSize = 10 * 1024 * 1024; // 10MB
            public static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
        }
    }
}