using Microsoft.AspNetCore.Mvc;
using EforTakipUygulamasi.Common;
using EforTakipUygulamasi.Models;

namespace EforTakipUygulamasi.Controllers
{
    [RequireLogin] // Giriş yapmış olmalı
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: User/Index - Kullanıcı listesi (Sadece Admin)
        public IActionResult Index()
        {
            // Admin kontrolü
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok";
                return RedirectToAction("Index", "Request");
            }

            try
            {
                var users = _userRepository.GetAll()
                    .OrderBy(u => u.CreatedDate)
                    .ToList();

                return View(users);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kullanıcılar yüklenirken hata: {ex.Message}";
                return View(new List<User>());
            }
        }

        // GET: User/Create - Yeni kullanıcı oluştur (Sadece Admin)
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok";
                return RedirectToAction("Index", "Request");
            }

            var model = new User
            {
                Role = UserRole.Viewer // Default rol
            };
            return View(model);
        }

        // POST: User/Create - Yeni kullanıcı kaydet (Sadece Admin)
        [HttpPost]
        public IActionResult Create(User user, string password, string confirmPassword)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok";
                return RedirectToAction("Index", "Request");
            }

            try
            {
                // Şifre kontrolleri
                if (string.IsNullOrEmpty(password))
                {
                    ViewBag.ErrorMessage = "Şifre zorunludur";
                    return View(user);
                }

                if (password != confirmPassword)
                {
                    ViewBag.ErrorMessage = "Şifreler uyuşmuyor";
                    return View(user);
                }

                if (!PasswordHelper.IsPasswordStrong(password))
                {
                    ViewBag.ErrorMessage = PasswordHelper.GetPasswordStrengthMessage(password);
                    return View(user);
                }

                // Kullanıcı adı kontrolü
                if (_userRepository.IsUsernameExists(user.Username))
                {
                    ViewBag.ErrorMessage = "Bu kullanıcı adı zaten kullanılıyor";
                    return View(user);
                }

                // Email kontrolü
                if (_userRepository.IsEmailExists(user.Email))
                {
                    ViewBag.ErrorMessage = "Bu email adresi zaten kullanılıyor";
                    return View(user);
                }

                // ModelState kontrolü
                if (!ModelState.IsValid)
                {
                    ViewBag.ErrorMessage = "Lütfen tüm gerekli alanları doldurun";
                    return View(user);
                }

                // Oluşturan bilgisini session'dan al
                var currentUserName = HttpContext.Session.GetString("FullName") ?? "System";

                // Şifreyi hash'le ve bilgileri ayarla
                user.PasswordHash = PasswordHelper.HashPassword(password);
                user.CreatedDate = DateTime.Now;
                user.IsActive = true;
                user.FailedLoginAttempts = 0;
                user.IsLocked = false;

                _userRepository.Create(user);

                Console.WriteLine($"Yeni kullanıcı oluşturuldu: {user.Username} - {user.RoleDisplayName} (Oluşturan: {currentUserName})");
                TempData["SuccessMessage"] = $"✅ '{user.FullName}' kullanıcısı başarıyla oluşturuldu";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanıcı oluşturma hatası: {ex.Message}");
                ViewBag.ErrorMessage = $"Kullanıcı oluştururken hata: {ex.Message}";
                return View(user);
            }
        }

        // GET: User/Edit/5 - Kullanıcı düzenle (Admin veya kendi profilini düzenleyen)
        public IActionResult Edit(int id)
        {
            try
            {
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                var currentUserRole = HttpContext.Session.GetInt32("UserRole");

                // Admin değilse sadece kendi profilini düzenleyebilir
                if (currentUserRole != 1 && currentUserId != id)
                {
                    TempData["ErrorMessage"] = "Sadece kendi profilinizi düzenleyebilirsiniz";
                    return RedirectToAction("Index", "Request");
                }

                var user = _userRepository.GetById(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/Edit/5 - Kullanıcı güncelle (Admin veya kendi profilini güncelleyen)
        [HttpPost]
        public IActionResult Edit(User user)
        {
            try
            {
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                var currentUserRole = HttpContext.Session.GetInt32("UserRole");

                // Admin değilse sadece kendi profilini düzenleyebilir
                if (currentUserRole != 1 && currentUserId != user.Id)
                {
                    TempData["ErrorMessage"] = "Sadece kendi profilinizi düzenleyebilirsiniz";
                    return RedirectToAction("Index", "Request");
                }

                var existingUser = _userRepository.GetById(user.Id);
                if (existingUser == null)
                {
                    ViewBag.ErrorMessage = "Kullanıcı bulunamadı";
                    return View(user);
                }

                // Eğer admin değilse rol değiştiremesin
                if (currentUserRole != 1)
                {
                    user.Role = existingUser.Role;
                }

                // Kullanıcı adı kontrolü (kendisi hariç)
                if (_userRepository.IsUsernameExists(user.Username, user.Id))
                {
                    ViewBag.ErrorMessage = "Bu kullanıcı adı zaten kullanılıyor";
                    return View(user);
                }

                // Email kontrolü (kendisi hariç)
                if (_userRepository.IsEmailExists(user.Email, user.Id))
                {
                    ViewBag.ErrorMessage = "Bu email adresi zaten kullanılıyor";
                    return View(user);
                }

                // Mevcut şifre ve güvenlik bilgilerini koru
                user.PasswordHash = existingUser.PasswordHash;
                user.CreatedDate = existingUser.CreatedDate;
                user.FailedLoginAttempts = existingUser.FailedLoginAttempts;
                user.LastFailedLogin = existingUser.LastFailedLogin;
                user.IsLocked = existingUser.IsLocked;
                user.LockoutEnd = existingUser.LockoutEnd;
                user.LastLogin = existingUser.LastLogin;

                _userRepository.Update(user);

                TempData["SuccessMessage"] = $"✅ '{user.FullName}' kullanıcısı başarıyla güncellendi";

                // Admin ise kullanıcı listesine, değilse ana sayfaya git
                if (currentUserRole == 1)
                    return RedirectToAction(nameof(Index));
                else
                    return RedirectToAction("Index", "Request");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Kullanıcı güncellenirken hata: {ex.Message}";
                return View(user);
            }
        }

        // POST: User/ChangePassword/5 - Şifre değiştir (Admin veya kendisi)
        [HttpPost]
        public JsonResult ChangePassword(int id, string newPassword, string confirmPassword)
        {
            try
            {
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                var currentUserRole = HttpContext.Session.GetInt32("UserRole");

                // Admin değilse sadece kendi şifresini değiştirebilir
                if (currentUserRole != 1 && currentUserId != id)
                {
                    return Json(new { success = false, message = "Sadece kendi şifrenizi değiştirebilirsiniz" });
                }

                if (newPassword != confirmPassword)
                {
                    return Json(new { success = false, message = "Şifreler uyuşmuyor" });
                }

                if (!PasswordHelper.IsPasswordStrong(newPassword))
                {
                    return Json(new { success = false, message = PasswordHelper.GetPasswordStrengthMessage(newPassword) });
                }

                var user = _userRepository.GetById(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı" });
                }

                user.PasswordHash = PasswordHelper.HashPassword(newPassword);
                _userRepository.Update(user);

                Console.WriteLine($"Şifre değiştirildi: {user.Username}");
                return Json(new { success = true, message = $"✅ {user.FullName} kullanıcısının şifresi başarıyla değiştirildi" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Şifre değiştirme hatası: {ex.Message}");
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // POST: User/ToggleStatus/5 - Kullanıcı aktiflik durumu değiştir (Sadece Admin)
        [HttpPost]
        public JsonResult ToggleStatus(int id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Bu işlem için yetkiniz yok" });
            }

            try
            {
                var user = _userRepository.GetById(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı" });
                }

                user.IsActive = !user.IsActive;
                _userRepository.Update(user);

                var statusText = user.IsActive ? "aktif" : "pasif";
                Console.WriteLine($"Kullanıcı durumu değiştirildi: {user.Username} -> {statusText}");

                return Json(new
                {
                    success = true,
                    message = $"✅ {user.FullName} kullanıcısı {statusText} duruma getirildi",
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Durum değiştirme hatası: {ex.Message}");
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // POST: User/Delete/5 - Kullanıcı sil (Sadece Admin)
        [HttpPost]
        public JsonResult Delete(int id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Bu işlem için yetkiniz yok" });
            }

            try
            {
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if (id == currentUserId)
                {
                    return Json(new { success = false, message = "Kendi hesabınızı silemezsiniz" });
                }

                var user = _userRepository.GetById(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı" });
                }

                var userName = user.FullName;
                _userRepository.Delete(id);

                Console.WriteLine($"Kullanıcı silindi: {user.Username}");
                return Json(new { success = true, message = $"✅ '{userName}' kullanıcısı başarıyla silindi" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanıcı silme hatası: {ex.Message}");
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // Helper method - Admin kontrolü
        private bool IsAdmin()
        {
            var userRole = HttpContext.Session.GetInt32("UserRole");
            return userRole == (int)UserRole.Admin;
        }
    }
}