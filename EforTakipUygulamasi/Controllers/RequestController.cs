using Microsoft.AspNetCore.Mvc;
using EforTakipUygulamasi.Common;
using EforTakipUygulamasi.Models;

namespace EforTakipUygulamasi.Controllers
{
    public class RequestController : Controller
    {
        private readonly IRequestRepository _repository;

        public RequestController(IRequestRepository repository)
        {
            _repository = repository;
        }

        // GÜNCELLEME: Ana sayfa - sadece bekleyen, tamamlanan, iptal
        public IActionResult Index()
        {
            try
            {
                var requests = _repository.GetAll();

                // Sadece aktif olmayan talepleri göster
                var filteredRequests = requests
                    .Where(r => !r.IsActive) // InProgress ve Testing hariç
                    .OrderBy(r => r.Status == RequestStatusEnum.New ? 0 :
                                 r.Status == RequestStatusEnum.Completed ? 1 : 2)
                    .ThenByDescending(r => r.CreatedDate)
                    .ToList();

                ViewBag.PageTitle = "Talepler";
                ViewBag.IsActivePage = false;

                return View(filteredRequests);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Veri yüklenirken hata: {ex.Message}";
                return View(new List<Request>());
            }
        }

        // YENİ: Aktif talepler sayfası
        public IActionResult Active()
        {
            try
            {
                var requests = _repository.GetAll();

                // Sadece aktif talepleri göster (InProgress ve Testing)
                var activeRequests = requests
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.Deadline ?? DateTime.MaxValue) // Deadline'e göre sırala
                    .ToList();

                ViewBag.PageTitle = "Aktif Talepler";
                ViewBag.IsActivePage = true;

                return View("Index", activeRequests); // Aynı view'i kullan
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Aktif talepler yüklenirken hata: {ex.Message}";
                return View("Index", new List<Request>());
            }
        }

        // YENİ: AJAX ile durum değiştirme
        [HttpPost]
        public JsonResult ChangeStatus(int id, int newStatus)
        {
            try
            {
                var request = _repository.GetById(id);
                if (request == null)
                {
                    return Json(new { success = false, message = "Talep bulunamadı." });
                }

                var oldStatus = request.Status;
                request.Status = (RequestStatusEnum)newStatus;
                request.LastModified = DateTime.Now;
                request.LastModifiedBy = "Koray";

                _repository.Update(request);

                var statusText = EffortHelper.StatusToString(request.Status);

                return Json(new
                {
                    success = true,
                    message = $"Durum '{statusText}' olarak güncellendi.",
                    newStatusText = statusText,
                    needsPageRefresh = (oldStatus == RequestStatusEnum.InProgress || oldStatus == RequestStatusEnum.Testing) !=
                                    (request.Status == RequestStatusEnum.InProgress || request.Status == RequestStatusEnum.Testing)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        public IActionResult Create()
        {
            var model = new Request();

            // Otomatik ID öner
            var allRequests = _repository.GetAll();
            ViewBag.SuggestedProjectId = EffortHelper.GenerateRequestId(allRequests);

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(Request request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Eğer ProjectId boşsa otomatik ata
                    if (string.IsNullOrWhiteSpace(request.ProjectId))
                    {
                        var allRequests = _repository.GetAll();
                        request.ProjectId = EffortHelper.GenerateRequestId(allRequests);
                    }

                    request.Status = RequestStatusEnum.New;
                    request.CreatedDate = DateTime.Now;
                    request.LastModified = DateTime.Now;
                    request.CreatedBy = "Koray";

                    _repository.Create(request);

                    TempData["SuccessMessage"] = "İş başarıyla oluşturuldu!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kayıt sırasında hata: {ex.Message}";
            }

            // Hata durumunda tekrar ID öner
            var allRequestsForError = _repository.GetAll();
            ViewBag.SuggestedProjectId = EffortHelper.GenerateRequestId(allRequestsForError);

            return View(request);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                Console.WriteLine($"Edit method çağrıldı, ID: {id}");

                var request = _repository.GetById(id);
                if (request == null)
                {
                    Console.WriteLine($"Request bulunamadı: ID={id}");
                    TempData["ErrorMessage"] = "İş bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                Console.WriteLine($"Request bulundu: {request.Name}");
                return View(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Edit method hatası: {ex.Message}");
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public IActionResult Edit(Request request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    request.LastModified = DateTime.Now;
                    request.LastModifiedBy = "Koray";

                    _repository.Update(request);

                    TempData["SuccessMessage"] = "İş başarıyla güncellendi!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Güncelleme sırasında hata: {ex.Message}";
            }

            return View(request);
        }

        public IActionResult Delete(int id)
        {
            try
            {
                var request = _repository.GetById(id);
                if (request == null)
                {
                    TempData["ErrorMessage"] = "İş bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                return View(request);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _repository.Delete(id);
                TempData["SuccessMessage"] = "İş başarıyla silindi!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Silme sırasında hata: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Dashboard için JSON API - GÜNCELLEME: Yaklaşan deadline hesabı
        [HttpGet]
        public JsonResult GetAllJson()
        {
            try
            {
                var requests = _repository.GetAll();
                Console.WriteLine($"GetAllJson çağrıldı: {requests.Count} request bulundu");

                return Json(requests);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllJson Hatası: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }

        // YENİ: Dosya upload endpoint (placeholder)
        [HttpPost]
        public JsonResult UploadFile(int requestId, IFormFile file)
        {
            try
            {
                // Şimdilik mock response
                return Json(new
                {
                    success = true,
                    fileName = file.FileName,
                    message = "Dosya başarıyla yüklendi (mock)"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // YENİ: Dosya silme endpoint (placeholder)
        [HttpPost]
        public JsonResult DeleteFile(int requestId, string fileName)
        {
            try
            {
                // Şimdilik mock response
                return Json(new
                {
                    success = true,
                    message = "Dosya başarıyla silindi (mock)"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}