using EforTakipUygulamasi.Models;

namespace EforTakipUygulamasi.Common
{
    public interface IRequestService
    {
        // CRUD Operations
        List<Request> GetAllRequests();
        Request? GetRequest(int id);
        Request CreateRequest(Request request);
        Request UpdateRequest(Request request);
        void DeleteRequest(int id);

        // Business Logic
        List<Request> GetRequestsByStatus(string status);
        List<Request> GetOverdueRequests();
        List<Request> GetMyRequests(string userId);

        // Dashboard Data
        DashboardData GetDashboardData();
        List<Request> GetRecentRequests(int count = 10);

        // Validation & Business Rules
        bool CanUserEditRequest(int requestId, string userId, string userRole);
        bool IsRequestNameUnique(string name, int? excludeId = null);
        void RecalculateEffortMetrics(Request request);
    }

    // Dashboard için veri transfer objesi
    public class DashboardData
    {
        public int TotalRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int OverdueRequests { get; set; }
        public double TotalHours { get; set; }
        public Dictionary<TShirtSizeEnum, int> TShirtDistribution { get; set; } = new();
        public List<Request> RecentRequests { get; set; } = new();
    }
}