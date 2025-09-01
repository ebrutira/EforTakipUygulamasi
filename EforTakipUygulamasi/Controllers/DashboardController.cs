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

                // GÜNCELLEME: Geciken → Yaklaşan hesaplaması
                var stats = new DashboardStats
                {
                    TotalRequests = requests.Count,
                    InProgressRequests = requests.Count(r => r.Status == RequestStatusEnum.InProgress),
                    TotalHours = (double)requests.Sum(r => r.TotalHours),
                    OverdueRequests = requests.Count(r => r.IsOverdue &&
                        r.Status != RequestStatusEnum.Completed &&
                        r.Status != RequestStatusEnum.Cancelled)
                };

                ViewBag.Stats = stats;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Dashboard yüklenirken hata: {ex.Message}";
                ViewBag.Stats = new DashboardStats();
                return View();
            }
        }

        // YENİ: Dashboard istatistikleri için API
        [HttpGet]
        public JsonResult GetDashboardStats()
        {
            try
            {
                var requests = _repository.GetAll();

                var stats = new
                {
                    TotalRequests = requests.Count,
                    InProgressRequests = requests.Count(r => r.Status == RequestStatusEnum.InProgress),
                    TestingRequests = requests.Count(r => r.Status == RequestStatusEnum.Testing),
                    CompletedRequests = requests.Count(r => r.Status == RequestStatusEnum.Completed),
                    PendingRequests = requests.Count(r => r.Status == RequestStatusEnum.New),
                    CancelledRequests = requests.Count(r => r.Status == RequestStatusEnum.Cancelled),
                    TotalHours = requests.Sum(r => r.TotalHours),
                    TotalManDays = EffortHelper.CalculateManDays(requests.Sum(r => r.TotalHours)),

                    // GÜNCELLEME: Yaklaşan deadline hesabı (aktif işlerde)
                    ApproachingDeadlines = EffortHelper.GetApproachingCount(requests),

                    // Geciken sadece dashboard için
                    OverdueRequests = requests.Count(r => r.IsOverdue &&
                        r.Status != RequestStatusEnum.Completed &&
                        r.Status != RequestStatusEnum.Cancelled),

                    // Aktif iş sayısı
                    ActiveRequests = requests.Count(r => r.IsActive)
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}