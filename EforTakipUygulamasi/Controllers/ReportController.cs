using EforTakipUygulamasi.Models;
using EforTakipUygulamasi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EforTakipUygulamasi.Controllers
{
    public class ReportController : Controller
    {
        private readonly IRequestRepository _requestRepository;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IRequestRepository requestRepository, ILogger<ReportController> logger)
        {
            _requestRepository = requestRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var requests = await _requestRepository.GetAllAsync();
                var requestsList = requests.ToList();

                var reportData = new
                {
                    TotalRequests = requestsList.Count,
                    CompletedRequests = requestsList.Count(r => r.Status == RequestStatus.Completed),
                    InProgressRequests = requestsList.Count(r => r.Status == RequestStatus.InProgress),
                    OverdueRequests = requestsList.Count(r => r.Deadline.HasValue && r.Deadline.Value.Date < DateTime.Today && r.Status != RequestStatus.Completed),
                    TotalEffort = requestsList.Sum(r => r.TotalHours),
                    AvgEffortPerRequest = requestsList.Any() ? requestsList.Average(r => r.TotalHours) : 0,

                    // Efor dağılımı
                    AnalystHours = requestsList.Sum(r => r.AnalystHours),
                    DeveloperHours = requestsList.Sum(r => r.DeveloperHours),
                    KKTHours = requestsList.Sum(r => r.KKTHours),
                    PreprodHours = requestsList.Sum(r => r.PreprodHours),

                    // Bu ay oluşturulan talepler
                    ThisMonthRequests = requestsList.Count(r => r.CreatedDate.Month == DateTime.Now.Month && r.CreatedDate.Year == DateTime.Now.Year),

                    // Bu hafta oluşturulan talepler
                    ThisWeekRequests = requestsList.Count(r => r.CreatedDate >= DateTime.Now.AddDays(-7)),

                    // En büyük işler
                    LargestRequests = requestsList.OrderByDescending(r => r.TotalHours).Take(5).ToList(),

                    // Yaklaşan deadline'lar (sadece deadline'ı olan işler)
                    UpcomingDeadlines = requestsList
                        .Where(r => r.Deadline.HasValue &&
                                   r.Deadline.Value.Date >= DateTime.Today &&
                                   r.Deadline.Value.Date <= DateTime.Today.AddDays(7) &&
                                   r.Status != RequestStatus.Completed &&
                                   r.Status != RequestStatus.Cancelled)
                        .OrderBy(r => r.Deadline)
                        .ToList()
                };

                ViewBag.ReportData = reportData;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Raporlar yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Raporlar yüklenirken hata oluştu: " + ex.Message;
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportData(string format = "json")
        {
            try
            {
                var requests = await _requestRepository.GetAllAsync();
                var requestsList = requests.ToList();

                if (format.ToLower() == "csv")
                {
                    var csv = GenerateCSV(requestsList);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                    return File(bytes, "text/csv", $"EforTakip_{DateTime.Now:yyyyMMdd_HHmm}.csv");
                }
                else
                {
                    var exportData = requestsList.Select(r => new
                    {
                        Id = r.Id,
                        Ad = r.Name,
                        Aciklama = r.Description,
                        Durum = r.Status.ToString(),
                        Oncelik = r.Priority.ToString(),
                        AnalistSaat = r.AnalystHours,
                        YazilimSaat = r.DeveloperHours,
                        KKTSaat = r.KKTHours,
                        PreprodSaat = r.PreprodHours,
                        ToplamSaat = r.TotalHours,
                        AdamGun = r.TotalManDays,
                        Buyukluk = r.Size.ToString(),
                        StoryPoints = r.StoryPoints,
                        Deadline = r.Deadline?.ToString("yyyy-MM-dd"),
                        KKTDeadline = r.KKTDeadline?.ToString("yyyy-MM-dd"),
                        Olusturan = r.CreatedBy,
                        OlusturmaTarihi = r.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                        SonDegisiklik = r.LastModified.ToString("yyyy-MM-dd HH:mm"),
                        SonDegistiren = r.LastModifiedBy
                    });

                    var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                    var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                    return File(bytes, "application/json", $"EforTakip_{DateTime.Now:yyyyMMdd_HHmm}.json");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Veri export edilirken hata oluştu");
                TempData["ErrorMessage"] = "Export işleminde hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private string GenerateCSV(IEnumerable<Request> requests)
        {
            var csv = new StringBuilder();

            // Başlık satırı - Türkçe ve düzenli
            csv.AppendLine("TALEP_NO,TALEP_ADI,ACIKLAMA,DURUM,ONCELIK,ANALIST_SAAT,YAZILIM_SAAT,KKT_SAAT,PREPROD_SAAT,TOPLAM_SAAT,ADAM_GUN,BUYUKLUK,STORY_POINTS,DEADLINE,KKT_DEADLINE,OLUSTURAN,OLUSTURMA_TARIHI,SON_DEGISTIREN,SON_DEGISIKLIK");

            foreach (var request in requests)
            {
                // Verileri temizle ve formatla
                var name = CleanCsvField(request.Name);
                var description = CleanCsvField(request.Description);
                var status = GetStatusTextTurkish(request.Status);
                var priority = GetPriorityTextTurkish(request.Priority);
                var size = request.Size.ToString();
                var deadline = request.Deadline?.ToString("dd.MM.yyyy") ?? "";
                var kktDeadline = request.KKTDeadline?.ToString("dd.MM.yyyy") ?? "";
                var createdBy = CleanCsvField(request.CreatedBy);
                var lastModifiedBy = CleanCsvField(request.LastModifiedBy);
                var createdDate = request.CreatedDate.ToString("dd.MM.yyyy HH:mm");
                var lastModified = request.LastModified.ToString("dd.MM.yyyy HH:mm");

                csv.AppendLine($"{request.Id},{name},{description},{status},{priority},{request.AnalystHours},{request.DeveloperHours},{request.KKTHours},{request.PreprodHours},{request.TotalHours:F1},{request.TotalManDays:F1},{size},{request.StoryPoints},{deadline},{kktDeadline},{createdBy},{createdDate},{lastModifiedBy},{lastModified}");
            }

            return csv.ToString();
        }

        private string CleanCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";

            // CSV için özel karakterleri temizle
            field = field.Replace("\"", "\"\""); // Çift tırnakları escape et
            field = field.Replace("\n", " ").Replace("\r", " "); // Satır sonlarını kaldır

            // Eğer virgül, çift tırnak veya satır sonu varsa, alanı çift tırnakla sar
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                field = $"\"{field}\"";
            }

            return field;
        }

        private string GetStatusTextTurkish(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.New => "Yeni",
                RequestStatus.InProgress => "Devam_Eden",
                RequestStatus.Testing => "Test",
                RequestStatus.Completed => "Tamamlandi",
                RequestStatus.Cancelled => "Iptal",
                _ => status.ToString()
            };
        }

        private string GetPriorityTextTurkish(RequestPriority priority)
        {
            return priority switch
            {
                RequestPriority.Low => "Dusuk",
                RequestPriority.Medium => "Orta",
                RequestPriority.High => "Yuksek",
                RequestPriority.Critical => "Kritik",
                _ => priority.ToString()
            };
        }
    }
}