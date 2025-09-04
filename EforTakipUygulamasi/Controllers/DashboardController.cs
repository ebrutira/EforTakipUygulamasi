using Microsoft.AspNetCore.Mvc;
using EforTakipUygulamasi.Common;
using EforTakipUygulamasi.Models;

namespace EforTakipUygulamasi.Controllers
{
    [RequireLogin] // Giriş yapmış kullanıcılar için
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
                    ApproachingDeadlines = EffortHelper.GetApproachingCount(requests),
                    OverdueRequests = requests.Count(r => r.IsOverdue &&
                        r.Status != RequestStatusEnum.Completed &&
                        r.Status != RequestStatusEnum.Cancelled),
                    ActiveRequests = requests.Count(r => r.IsActive)
                };

                ViewBag.Stats = stats;
                ViewBag.RecentRequests = requests
                    .OrderByDescending(r => r.CreatedDate)
                    .Take(10)
                    .ToList();

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Dashboard yüklenirken hata: {ex.Message}";
                ViewBag.Stats = new { };
                ViewBag.RecentRequests = new List<Request>();
                return View();
            }
        }

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

                    ApproachingDeadlines = EffortHelper.GetApproachingCount(requests),

                    OverdueRequests = requests.Count(r => r.IsOverdue &&
                        r.Status != RequestStatusEnum.Completed &&
                        r.Status != RequestStatusEnum.Cancelled),

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