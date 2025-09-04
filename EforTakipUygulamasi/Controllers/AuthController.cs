using Microsoft.AspNetCore.Mvc;
using EforTakipUygulamasi.Common;
using EforTakipUygulamasi.Models;

namespace EforTakipUygulamasi.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AuthController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Eğer zaten giriş yapmışsa dashboard'a yönlendir
            if (IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Request");
            }

            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            try
            {
                Console.WriteLine($"Login denemesi: {username}");

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ViewBag.ErrorMessage = "Kullanıcı adı ve şifre zorunludur";
                    return View();
                }

                var user = _userRepository.GetByUsername(username);

                if (user == null)
                {
                    Console.WriteLine($"Kullanıcı bulunamadı: {username}");
                    ViewBag.ErrorMessage = "Kullanıcı adı veya şifre hatalı";
                    return View();
                }

                // Hesap kilitli mi kontrol et
                if (user.IsAccountLocked)
                {
                    ViewBag.ErrorMessage = "Hesabınız geçici olarak kilitli";
                    return View();
                }

                // Şifre kontrolü - GEÇİCİ ÇÖZÜM
                bool isPasswordValid = false;

                // Eğer PasswordHash basit textse (admin123 gibi), direkt karşılaştır
                if (user.PasswordHash.Length < 50) // Hash değilse
                {
                    isPasswordValid = password == user.PasswordHash;
                }
                else // Hash'lenmiş şifreyse BCrypt kullan
                {
                    isPasswordValid = PasswordHelper.VerifyPassword(password, user.PasswordHash);
                }

                if (!isPasswordValid)
                {
                    Console.WriteLine($"Şifre hatalı: {username}");

                    // Başarısız deneme sayısını artır
                    user.FailedLoginAttempts++;
                    user.LastFailedLogin = DateTime.Now;

                    // 5 başarısız denemeden sonra 15 dakika kilitle
                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.IsLocked = true;
                        user.LockoutEnd = DateTime.Now.AddMinutes(15);
                        Console.WriteLine($"Kullanıcı kilitlendi: {username}");
                    }

                    _userRepository.Update(user);
                    ViewBag.ErrorMessage = "Kullanıcı adı veya şifre hatalı";
                    return View();
                }

                // Başarılı giriş
                Console.WriteLine($"Başarılı giriş: {username}");

                user.LastLogin = DateTime.Now;
                user.FailedLoginAttempts = 0;
                user.IsLocked = false;
                user.LockoutEnd = null;
                _userRepository.Update(user);

                // Session'a kullanıcı bilgisini kaydet
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetInt32("UserRole", (int)user.Role);

                TempData["SuccessMessage"] = $"Hoş geldiniz, {user.FullName}!";

                return RedirectToAction("Index", "Request");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login hatası: {ex.Message}");
                ViewBag.ErrorMessage = "Giriş sırasında bir hata oluştu";
                return View();
            }
        }

        // POST: Auth/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["InfoMessage"] = "Başarıyla çıkış yaptınız";
            return RedirectToAction("Login");
        }

        // Helper method - kullanıcı giriş yapmış mı kontrol et
        private bool IsUserLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId").HasValue;
        }
    }
}