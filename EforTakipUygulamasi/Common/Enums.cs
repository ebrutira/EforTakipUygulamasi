namespace EforTakipUygulamasi.Common
{
    public enum RequestStatusEnum
    {
        New = 1,        // Bekleyen (eski: Yeni)
        InProgress = 2, // Devam Eden
        Testing = 3,    // Test
        OnHold = 4,     // Beklemede (Bu durumu kaldıracağız, sadece uyumluluk için)
        Completed = 5,  // Tamamlandı
        Cancelled = 6   // İptal
    }

    public enum PriorityLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum TShirtSizeEnum
    {
        FastTrack,
        XS,
        S,
        M,
        L,
        XL
    }

    public enum UserRole
    {
        Admin = 1,
        Contributor = 2,
        Viewer = 3
    }

    // Yeni: Deadline durum enum'u
    public enum DeadlineStatus
    {
        Overdue,     // Gecikmiş (kırmızı)
        Critical,    // 2 hafta kaldı (kırmızı)
        Warning,     // 4 hafta kaldı (turuncu)
        Caution,     // 6 hafta kaldı (sarı)
        Normal       // 8+ hafta (yeşil)
    }
}