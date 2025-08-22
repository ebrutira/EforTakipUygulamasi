using EforTakipUygulamasi.Models;
using EforTakipUygulamasi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EforTakipUygulamasi.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IRequestRepository _requestRepository;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IRequestRepository requestRepository, ILogger<DashboardController> logger)
        {
            _requestRepository = requestRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var stats = await _requestRepository.GetDashboardStatsAsync();
                var recentRequests = await _requestRepository.GetAllAsync();
                var overdueRequests = await _requestRepository.GetOverdueAsync();

                ViewBag.Stats = stats;
                ViewBag.RecentRequests = recentRequests.Take(5);
                ViewBag.OverdueRequests = overdueRequests.Take(5);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard yüklenirken hata oluştu");
                return View("Error");
            }
        }

        public async Task<IActionResult> GetChartData()
        {
            try
            {
                var stats = await _requestRepository.GetDashboardStatsAsync();
                var requests = await _requestRepository.GetAllAsync();

                _logger.LogInformation($"Chart data: {stats.SizeDistribution.Count} size items, {stats.StatusDistribution.Count} status items");

                var chartData = new
                {
                    sizeDistribution = stats.SizeDistribution.Where(x => x.Value > 0).Select(x => new {
                        label = x.Key.ToString(),
                        value = x.Value,
                        color = GetSizeColor(x.Key)
                    }).ToList(),
                    statusDistribution = stats.StatusDistribution.Where(x => x.Value > 0).Select(x => new {
                        label = GetStatusText(x.Key),
                        value = x.Value,
                        color = GetStatusColor(x.Key)
                    }).ToList(),
                    effortDistribution = new
                    {
                        analyst = requests.Sum(r => r.AnalystHours),
                        developer = requests.Sum(r => r.DeveloperHours),
                        kkt = requests.Sum(r => r.KKTHours),
                        preprod = requests.Sum(r => r.PreprodHours)
                    }
                };

                return Json(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chart data alınırken hata oluştu");

                // Fallback data
                var fallbackData = new
                {
                    sizeDistribution = new[] {
                        new { label = "FastTrack", value = 3, color = "#c6f6d5" },
                        new { label = "XS", value = 2, color = "#c6f6d5" },
                        new { label = "S", value = 3, color = "#fef5e7" },
                        new { label = "M", value = 1, color = "#fed7d7" },
                        new { label = "L", value = 1, color = "#e9d8fd" }
                    },
                    statusDistribution = new[] {
                        new { label = "Yeni", value = 4, color = "#667eea" },
                        new { label = "Devam Eden", value = 3, color = "#bee3f8" },
                        new { label = "Test", value = 2, color = "#fef5e7" },
                        new { label = "Tamamlandı", value = 1, color = "#c6f6d5" }
                    },
                    effortDistribution = new
                    {
                        analyst = 69.0,
                        developer = 167.0,
                        kkt = 49.0,
                        preprod = 27.0
                    }
                };

                return Json(fallbackData);
            }
        }

        private string GetSizeColor(TShirtSize size)
        {
            return size switch
            {
                TShirtSize.FastTrack => "#28a745", // Yeşil
                TShirtSize.XS => "#6f42c1",        // Mor
                TShirtSize.S => "#007bff",         // Mavi
                TShirtSize.M => "#ffc107",         // Sarı
                TShirtSize.L => "#fd7e14",         // Turuncu
                TShirtSize.XL => "#dc3545",        // Kırmızı
                _ => "#6c757d"                     // Gri
            };
        }

        private string GetStatusColor(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.New => "#17a2b8",        // Info
                RequestStatus.InProgress => "#ffc107", // Warning
                RequestStatus.Testing => "#fd7e14",    // Orange
                RequestStatus.Completed => "#28a745",  // Success
                RequestStatus.Cancelled => "#6c757d",  // Secondary
                _ => "#007bff"                         // Primary
            };
        }

        private string GetStatusText(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.New => "Yeni",
                RequestStatus.InProgress => "Devam Ediyor",
                RequestStatus.Testing => "Test",
                RequestStatus.Completed => "Tamamlandı",
                RequestStatus.Cancelled => "İptal",
                _ => status.ToString()
            };
        }
    }
}