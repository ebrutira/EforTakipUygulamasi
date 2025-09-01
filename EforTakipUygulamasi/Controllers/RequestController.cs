using Microsoft.AspNetCore.Mvc;
using EforTakipUygulamasi.Common;
using EforTakipUygulamasi.Models;
using System.IO;

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

        // ====== DOSYA YÖNETİMİ METHOD'LARI ======

        [HttpPost]
        public async Task<JsonResult> UploadFile(int requestId, IFormFile file)
        {
            try
            {
                Console.WriteLine($"UploadFile çağrıldı: RequestId={requestId}, FileName={file?.FileName}");

                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Dosya seçilmedi." });
                }

                // Dosya boyutu kontrolü (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "Dosya boyutu 10MB'dan büyük olamaz." });
                }

                // Dosya uzantısı kontrolü
                var allowedExtensions = new[] { ".doc", ".docx", ".pdf", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".csv" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = $"Desteklenmeyen dosya formatı: {fileExtension}" });
                }

                // Upload klasörü oluştur
                var uploadsFolder = Path.Combine("wwwroot", "uploads", requestId.ToString());
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), uploadsFolder);

                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                    Console.WriteLine($"Klasör oluşturuldu: {fullPath}");
                }

                // Dosya adını güvenli hale getir
                var fileName = Path.GetFileName(file.FileName);
                var safeName = Path.GetFileNameWithoutExtension(fileName) + "_" + DateTime.Now.Ticks + fileExtension;
                var filePath = Path.Combine(fullPath, safeName);

                // Dosyayı kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                Console.WriteLine($"Dosya kaydedildi: {filePath} (Boyut: {file.Length} bytes)");

                return Json(new
                {
                    success = true,
                    fileName = fileName,
                    safeName = safeName,
                    message = "Dosya başarıyla yüklendi.",
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dosya yükleme hatası: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Dosya yüklenirken hata: {ex.Message}" });
            }
        }

        [HttpPost]
        public JsonResult DeleteFile(int requestId, string fileName)
        {
            try
            {
                Console.WriteLine($"DeleteFile çağrıldı: RequestId={requestId}, FileName={fileName}");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", requestId.ToString());

                if (!Directory.Exists(uploadsFolder))
                {
                    Console.WriteLine($"Klasör bulunamadı: {uploadsFolder}");
                    return Json(new { success = false, message = "Upload klasörü bulunamadı." });
                }

                // Dosyayı bul (güvenli dosya adları için pattern matching)
                var files = Directory.GetFiles(uploadsFolder, "*", SearchOption.TopDirectoryOnly);
                var fileToDelete = files.FirstOrDefault(f =>
                {
                    var fileNameOnly = Path.GetFileName(f);
                    var originalName = ExtractOriginalName(fileNameOnly);
                    return originalName.Equals(fileName, StringComparison.OrdinalIgnoreCase) ||
                           fileNameOnly.Contains(Path.GetFileNameWithoutExtension(fileName));
                });

                if (fileToDelete != null && System.IO.File.Exists(fileToDelete))
                {
                    System.IO.File.Delete(fileToDelete);
                    Console.WriteLine($"Dosya silindi: {fileToDelete}");

                    return Json(new { success = true, message = "Dosya başarıyla silindi." });
                }
                else
                {
                    Console.WriteLine($"Silinecek dosya bulunamadı: {fileName}");
                    var existingFiles = string.Join(", ", files.Select(Path.GetFileName));
                    Console.WriteLine($"Mevcut dosyalar: {existingFiles}");

                    return Json(new { success = false, message = "Dosya bulunamadı." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dosya silme hatası: {ex.Message}");
                return Json(new { success = false, message = $"Dosya silinirken hata: {ex.Message}" });
            }
        }

        [HttpGet]
        public JsonResult GetFiles(int requestId)
        {
            try
            {
                Console.WriteLine($"GetFiles çağrıldı: RequestId={requestId}");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", requestId.ToString());

                if (!Directory.Exists(uploadsFolder))
                {
                    Console.WriteLine($"Upload klasörü yok: {uploadsFolder}");
                    return Json(new { success = true, files = new List<object>() });
                }

                var files = Directory.GetFiles(uploadsFolder)
                    .Select(filePath => {
                        var fileInfo = new FileInfo(filePath);
                        var fileName = Path.GetFileName(filePath);
                        var originalName = ExtractOriginalName(fileName);

                        return new
                        {
                            fileName = fileName,
                            originalName = originalName,
                            fileSize = fileInfo.Length,
                            uploadDate = fileInfo.CreationTime
                        };
                    })
                    .OrderByDescending(f => f.uploadDate)
                    .ToList();

                Console.WriteLine($"Bulunan dosya sayısı: {files.Count}");
                foreach (var file in files)
                {
                    Console.WriteLine($"  - {file.originalName} ({file.fileSize} bytes)");
                }

                return Json(new { success = true, files });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dosya listesi alınamadı: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult DownloadFile(int requestId, string fileName)
        {
            try
            {
                Console.WriteLine($"DownloadFile çağrıldı: RequestId={requestId}, FileName={fileName}");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", requestId.ToString());
                var files = Directory.GetFiles(uploadsFolder, "*", SearchOption.TopDirectoryOnly);

                var fileToDownload = files.FirstOrDefault(f =>
                {
                    var fileNameOnly = Path.GetFileName(f);
                    var originalName = ExtractOriginalName(fileNameOnly);
                    return originalName.Equals(fileName, StringComparison.OrdinalIgnoreCase) ||
                           fileNameOnly.Contains(Path.GetFileNameWithoutExtension(fileName));
                });

                if (fileToDownload != null && System.IO.File.Exists(fileToDownload))
                {
                    var fileBytes = System.IO.File.ReadAllBytes(fileToDownload);
                    var originalName = ExtractOriginalName(Path.GetFileName(fileToDownload));
                    var contentType = GetContentType(originalName);

                    Console.WriteLine($"Dosya indiriliyor: {fileToDownload} -> {originalName}");

                    return File(fileBytes, contentType, originalName);
                }

                Console.WriteLine($"İndirilecek dosya bulunamadı: {fileName}");
                return NotFound("Dosya bulunamadı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dosya indirme hatası: {ex.Message}");
                return BadRequest($"Hata: {ex.Message}");
            }
        }

        // ====== HELPER METHODS ======

        private string ExtractOriginalName(string safeName)
        {
            try
            {
                // "document_637892345678901234.pdf" -> "document.pdf"
                var lastUnderscoreIndex = safeName.LastIndexOf('_');
                if (lastUnderscoreIndex > 0)
                {
                    var nameWithoutTicks = safeName.Substring(0, lastUnderscoreIndex);
                    var extension = Path.GetExtension(safeName);
                    return nameWithoutTicks + extension;
                }
                return safeName;
            }
            catch
            {
                return safeName;
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".png" => "image/png",
                ".csv" => "text/csv", 
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };
        }
    }
}