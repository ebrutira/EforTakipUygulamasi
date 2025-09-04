using BCrypt.Net;

namespace EforTakipUygulamasi.Common
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Şifre boş olamaz");

            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return false;

            bool hasLetter = password.Any(char.IsLetter);
            bool hasDigit = password.Any(char.IsDigit);

            return hasLetter && hasDigit;
        }

        public static string GetPasswordStrengthMessage(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "Şifre boş olamaz";

            if (password.Length < 6)
                return "Şifre en az 6 karakter olmalı";

            if (!password.Any(char.IsLetter))
                return "Şifre en az bir harf içermeli";

            if (!password.Any(char.IsDigit))
                return "Şifre en az bir rakam içermeli";

            return "Şifre uygun";
        }
    }
}