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

        public IActionResult Index()
        {
            try
            {
                var requests = _repository.GetAll();
                return View(requests);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Veri yüklenirken hata: {ex.Message}";
                return View(new List<Request>());
            }
        }

        public IActionResult Create()
        {
            return View(new Request());
        }

        [HttpPost]
        public IActionResult Create(Request request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    request.Status = RequestStatusEnum.New;
                    request.CreatedDate = DateTime.Now;
                    request.LastModified = DateTime.Now;
                    request.CreatedBy = "Koray"; // Şimdilik sabit

                    _repository.Create(request);

                    TempData["SuccessMessage"] = "İş başarıyla oluşturuldu!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kayıt sırasında hata: {ex.Message}";
            }

            return View(request);
        }

        public IActionResult Edit(int id)
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

        [HttpPost]
        public IActionResult Edit(Request request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    request.LastModified = DateTime.Now;
                    request.LastModifiedBy = "Koray"; // Şimdilik sabit

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

        // API endpoint - Dashboard için
        [HttpGet]
        public JsonResult GetAllJson()
        {
            try
            {
                var requests = _repository.GetAll();
                return Json(requests);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}