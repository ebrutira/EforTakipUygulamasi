using Microsoft.AspNetCore.Mvc;
using EforTakipUygulamasi.Common;
using EforTakipUygulamasi.Models;

namespace EforTakipUygulamasi.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IRequestRepository _repository;

        public DashboardController(IRequestRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            try
            {
                var requests = _repository.GetAll();

                // Dashboard Stats hesapla
                var stats = new DashboardStats
                {
                    TotalRequests = requests.Count,
                    InProgressRequests = requests.Count(r => r.Status == RequestStatusEnum.InProgress),
                    TotalHours = (double)requests.Sum(r => r.TotalHours),
                    OverdueRequests = requests.Count(r => r.IsOverdue)
                };

                var recentRequests = requests.OrderByDescending(r => r.CreatedDate).Take(10).ToList();
                var overdueRequests = requests.Where(r => r.IsOverdue).ToList();

                ViewBag.Stats = stats;
                ViewBag.RecentRequests = recentRequests;
                ViewBag.OverdueRequests = overdueRequests;

                return View();
            }
            catch (Exception ex)
            {
                // Debug için
                ViewBag.Error = $"Error: {ex.Message}";
                ViewBag.Stats = new DashboardStats();
                ViewBag.RecentRequests = new List<Request>();
                ViewBag.OverdueRequests = new List<Request>();
                return View();
            }
        }
    }
}
