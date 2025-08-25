using Microsoft.AspNetCore.Mvc;
using EforTakipUygulamasi.Common;
using EforTakipUygulamasi.Models;
using System.Text;

namespace EforTakipUygulamasi.Controllers
{
    public class ReportController : Controller
    {
        private readonly IRequestRepository _repository;

        public ReportController(IRequestRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            try
            {
                var requests = _repository.GetAll();

                // Rapor verilerini hazırla
                var reportData = new ReportData
                {
                    TotalRequests = requests.Count,
                    CompletedRequests = requests.Count(r => r.Status == RequestStatusEnum.Completed),
                    InProgressRequests = requests.Count(r => r.Status == RequestStatusEnum.InProgress),
                    OverdueRequests = requests.Count(r => r.IsOverdue),
                    TotalHours = requests.Sum(r => r.TotalHours),

                    // Efor dağılımı
                    AnalystTotalHours = requests.Sum(r => r.AnalystHours),
                    DeveloperTotalHours = requests.Sum(r => r.DeveloperHours),
                    KKTTotalHours = requests.Sum(r => r.KKTHours),
                    PreprodTotalHours = requests.Sum(r => r.PreprodHours),

                    // Performans metrikleri
                    ThisWeekCreated = requests.Count(r => r.CreatedDate >= DateTime.Now.AddDays(-7)),
                    ThisMonthCreated = requests.Count(r => r.CreatedDate.Month == DateTime.Now.Month),

                    CompletionRate = requests.Count > 0 ?
                        (requests.Count(r => r.Status == RequestStatusEnum.Completed) * 100.0 / requests.Count) : 0,
                    OverdueRate = requests.Count > 0 ?
                        (requests.Count(r => r.IsOverdue) * 100.0 / requests.Count) : 0
                };

                return View(reportData);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Rapor yüklenirken hata: {ex.Message}";
                return View(new ReportData());
            }
        }

        [HttpGet]
        public IActionResult ExportData(string format = "csv")
        {
            try
            {
                var requests = _repository.GetAll();

                if (format.ToLower() == "json")
                {
                    return Json(requests);
                }
                else // CSV
                {
                    var csv = new StringBuilder();
                    csv.AppendLine("ID,Ad,Durum,Öncelik,Analist,Yazılım,KKT,Preprod,Toplam,Oluşturma,Deadline");

                    foreach (var req in requests)
                    {
                        csv.AppendLine($"{req.Id}," +
                                     $"\"{req.Name}\"," +
                                     $"{req.Status}," +
                                     $"{req.Priority}," +
                                     $"{req.AnalystHours}," +
                                     $"{req.DeveloperHours}," +
                                     $"{req.KKTHours}," +
                                     $"{req.PreprodHours}," +
                                     $"{req.TotalHours}," +
                                     $"{req.CreatedDate:yyyy-MM-dd}," +
                                     $"{req.Deadline?.ToString("yyyy-MM-dd") ?? ""}");
                    }

                    var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                    return File(bytes, "text/csv", $"efor_raporu_{DateTime.Now:yyyyMMdd}.csv");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Export hatası: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }

    // Rapor için veri modeli
    public class ReportData
    {
        public int TotalRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int OverdueRequests { get; set; }
        public decimal TotalHours { get; set; }

        public decimal AnalystTotalHours { get; set; }
        public decimal DeveloperTotalHours { get; set; }
        public decimal KKTTotalHours { get; set; }
        public decimal PreprodTotalHours { get; set; }

        public int ThisWeekCreated { get; set; }
        public int ThisMonthCreated { get; set; }
        public double CompletionRate { get; set; }
        public double OverdueRate { get; set; }
    }
}