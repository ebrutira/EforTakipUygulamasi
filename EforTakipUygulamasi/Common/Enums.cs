namespace EforTakipUygulamasi.Common
{
    public enum RequestStatusEnum
    {
        New = 1,
        InProgress = 2,
        Testing = 3,
        OnHold = 4,        // ← EKLENDİ
        Completed = 5,     // ← 5'e değişti
        Cancelled = 6      // ← 6'ya değişti
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
}