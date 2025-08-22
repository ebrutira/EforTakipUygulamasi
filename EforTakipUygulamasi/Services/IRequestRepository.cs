using EforTakipUygulamasi.Models;

namespace EforTakipUygulamasi.Services
{
    public interface IRequestRepository
    {
        Task<IEnumerable<Request>> GetAllAsync();
        Task<Request?> GetByIdAsync(int id);
        Task<Request> AddAsync(Request request);
        Task<Request> UpdateAsync(Request request);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status);
        Task<IEnumerable<Request>> GetOverdueAsync();
        Task<DashboardStats> GetDashboardStatsAsync();
    }

    public class DashboardStats
    {
        public int TotalRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int OverdueRequests { get; set; }
        public decimal TotalHours { get; set; }
        public Dictionary<TShirtSize, int> SizeDistribution { get; set; } = new();
        public Dictionary<RequestStatus, int> StatusDistribution { get; set; } = new();
    }
}