using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EforTakipUygulamasi.Common
{
    // Giriş yapmamış kullanıcıları login sayfasına yönlendiren attribute
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                // Login sayfasına yönlendir
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }

            base.OnActionExecuting(context);
        }
    }

    // Admin yetkisi gerektiren sayfalar için
    public class RequireAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            var userRole = context.HttpContext.Session.GetInt32("UserRole");

            if (!userId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (userRole != 1) // Admin değilse
            {
                // Ana sayfaya yönlendir hata mesajıyla
                context.HttpContext.Session.SetString("ErrorMessage", "Bu sayfaya erişim yetkiniz yok");
                context.Result = new RedirectToActionResult("Index", "Request", null);
            }

            base.OnActionExecuting(context);
        }
    }
}