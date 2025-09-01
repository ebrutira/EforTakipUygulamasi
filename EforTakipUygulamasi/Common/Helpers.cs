using EforTakipUygulamasi.Models;

namespace EforTakipUygulamasi.Common
{
    public static class EffortHelper
    {
        public static string GetTShirtColor(TShirtSizeEnum size)
        {
            return size switch
            {
                TShirtSizeEnum.FastTrack => Constants.Colors.Green,
                TShirtSizeEnum.XS => Constants.Colors.Green,
                TShirtSizeEnum.S => Constants.Colors.Yellow,
                TShirtSizeEnum.M => Constants.Colors.Orange,
                TShirtSizeEnum.L => Constants.Colors.Red,
                TShirtSizeEnum.XL => Constants.Colors.Purple,
                _ => Constants.Colors.Red
            };
        }

        public static bool IsOverdue(DateTime? deadline)
        {
            return deadline.HasValue && deadline.Value < DateTime.Now;
        }

        // GÜNCELLEME: Yeni durum metni
        public static string StatusToString(RequestStatusEnum status)
        {
            return status switch
            {
                RequestStatusEnum.New => "Bekleyen",        // Değişti: Yeni → Bekleyen
                RequestStatusEnum.InProgress => "Devam Eden",
                RequestStatusEnum.Testing => "Test",
                RequestStatusEnum.OnHold => "Beklemede",     // Kullanılmayacak
                RequestStatusEnum.Completed => "Tamamlandı",
                RequestStatusEnum.Cancelled => "İptal",
                _ => "Bekleyen"
            };
        }

        // YENİ: Deadline durumu hesaplama
        public static DeadlineStatus GetDeadlineStatus(DateTime? deadline)
        {
            if (!deadline.HasValue) return DeadlineStatus.Normal;

            var today = DateTime.Today;
            var diffDays = (deadline.Value.Date - today).Days;

            if (diffDays < 0) return DeadlineStatus.Overdue;        // Gecikmiş
            if (diffDays <= 14) return DeadlineStatus.Critical;     // 2 hafta (kırmızı)
            if (diffDays <= 28) return DeadlineStatus.Warning;      // 4 hafta (turuncu)
            if (diffDays <= 42) return DeadlineStatus.Caution;      // 6 hafta (sarı)
            return DeadlineStatus.Normal;                           // 8+ hafta (yeşil)
        }

        // YENİ: Deadline CSS class
        public static string GetDeadlineClass(DateTime? deadline)
        {
            var status = GetDeadlineStatus(deadline);
            return status switch
            {
                DeadlineStatus.Overdue => "deadline-overdue",
                DeadlineStatus.Critical => "deadline-critical",
                DeadlineStatus.Warning => "deadline-warning",
                DeadlineStatus.Caution => "deadline-caution",
                DeadlineStatus.Normal => "deadline-normal",
                _ => "deadline-normal"
            };
        }

        // YENİ: Yaklaşan deadline sayısı
        public static int GetApproachingCount(List<Request> requests)
        {
            return requests.Count(r =>
                r.Deadline.HasValue &&
                (r.Status == RequestStatusEnum.InProgress || r.Status == RequestStatusEnum.Testing) &&
                GetDeadlineStatus(r.Deadline) == DeadlineStatus.Critical);
        }

        // YENİ: Adam-gün hesaplama
        public static decimal CalculateManDays(decimal totalHours)
        {
            return Math.Round(totalHours / 8, 1);
        }

        // YENİ: Otomatik ID oluştur
        public static string GenerateRequestId(List<Request> existingRequests)
        {
            // En büyük ID'yi bul ve +1 ekle, 6 haneli yap
            var maxId = existingRequests.Any() ? existingRequests.Max(r => r.Id) : 0;
            return (maxId + 1).ToString("D6"); // 000001, 000002, vs.
        }
    }
}