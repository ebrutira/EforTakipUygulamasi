using EforTakipUygulamasi.Models;
using EforTakipUygulamasi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EforTakipUygulamasi.Controllers
{
    public class RequestController : Controller
    {
        private readonly IRequestRepository _requestRepository;
        private readonly ILogger<RequestController> _logger;

        public RequestController(IRequestRepository requestRepository, ILogger<RequestController> logger)
        {
            _requestRepository = requestRepository;
            _logger = logger;
        }

        // GET: Request
        public async Task<IActionResult> Index()
        {
            try
            {
                var requests = await _requestRepository.GetAllAsync();
                return View(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Talepler listelenirken hata oluştu");
                return View("Error");
            }
        }

        // GET: Request/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(id);
                if (request == null)
                {
                    return NotFound();
                }
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Talep detayları alınırken hata oluştu: {RequestId}", id);
                return View("Error");
            }
        }

        // GET: Request/Create
        public IActionResult Create()
        {
            var model = new Request
            {
                CreatedBy = "Koray",
                LastModifiedBy = "Koray",
                Status = RequestStatus.New,
                Priority = RequestPriority.Medium
            };
            return View(model);
        }

        // POST: Request/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Request request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    request.CreatedBy = "Koray";
                    request.LastModifiedBy = "Koray";

                    await _requestRepository.AddAsync(request);

                    TempData["SuccessMessage"] = "Talep başarıyla oluşturuldu!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Talep oluşturulurken hata oluştu");
                TempData["ErrorMessage"] = "Talep oluşturulurken bir hata oluştu.";
            }

            return View(request);
        }

        // GET: Request/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(id);
                if (request == null)
                {
                    return NotFound();
                }
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Talep düzenlenirken hata oluştu: {RequestId}", id);
                return View("Error");
            }
        }

        // POST: Request/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Request request)
        {
            if (id != request.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    request.LastModifiedBy = "Koray";
                    await _requestRepository.UpdateAsync(request);

                    TempData["SuccessMessage"] = "Talep başarıyla güncellendi!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Talep güncellenirken hata oluştu: {RequestId}", id);
                TempData["ErrorMessage"] = "Talep güncellenirken bir hata oluştu.";
            }

            return View(request);
        }

        // GET: Request/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(id);
                if (request == null)
                {
                    return NotFound();
                }
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Talep silinirken hata oluştu: {RequestId}", id);
                return View("Error");
            }
        }

        // POST: Request/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _requestRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Talep başarıyla silindi!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Talep silinirken hata oluştu: {RequestId}", id);
                TempData["ErrorMessage"] = "Talep silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // API endpoint for getting requests as JSON
        [HttpGet]
        public async Task<IActionResult> GetRequestsJson()
        {
            try
            {
                var requests = await _requestRepository.GetAllAsync();
                var result = requests.Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    status = r.Status.ToString(),
                    statusText = GetStatusText(r.Status),
                    size = r.Size.ToString(),
                    analystHours = r.AnalystHours,
                    developerHours = r.DeveloperHours,
                    kktHours = r.KKTHours,
                    preprodHours = r.PreprodHours,
                    totalHours = r.TotalHours,
                    deadline = r.Deadline?.ToString("yyyy-MM-dd"),
                    deadlineFormatted = r.Deadline?.ToString("dd.MM.yyyy")
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JSON verisi alınırken hata oluştu");
                return BadRequest("Veri alınırken hata oluştu");
            }
        }

        private string GetStatusText(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.New => "Yeni",
                RequestStatus.InProgress => "Devam Eden",
                RequestStatus.Testing => "Test",
                RequestStatus.Completed => "Tamamlandı",
                RequestStatus.Cancelled => "İptal",
                _ => status.ToString()
            };
        }
    }
}