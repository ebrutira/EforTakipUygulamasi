namespace EforTakipUygulamasi.Models
{
    public class DashboardStats
    {
        public int TotalRequests { get; set; }
        public int InProgressRequests { get; set; }
        public double TotalHours { get; set; }
        public int OverdueRequests { get; set; }
    }
}