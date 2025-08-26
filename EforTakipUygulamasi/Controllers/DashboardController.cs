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

                // Basit istatistikler
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
    }
}