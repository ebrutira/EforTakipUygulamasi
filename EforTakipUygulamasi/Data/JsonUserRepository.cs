using System.Text.Json;
using EforTakipUygulamasi.Models;
using EforTakipUygulamasi.Common;

namespace EforTakipUygulamasi.Data
{
    public class JsonUserRepository : IUserRepository
    {
        private readonly string _usersFilePath = "Data/users.json";

        public List<User> GetAll()
        {
            try
            {
                Console.WriteLine($"User JSON dosyası aranıyor: {_usersFilePath}");

                if (!File.Exists(_usersFilePath))
                {
                    Console.WriteLine("User JSON dosyası bulunamadı, boş liste döndürülüyor.");
                    return new List<User>();
                }

                var json = File.ReadAllText(_usersFilePath);
                var users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();

                Console.WriteLine($"Toplam {users.Count} user yüklendi.");
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"User JSON okuma hatası: {ex.Message}");
                return new List<User>();
            }
        }

        public User? GetById(int id)
        {
            var users = GetAll();
            return users.FirstOrDefault(u => u.Id == id);
        }

        public User? GetByUsername(string username)
        {
            var users = GetAll();
            return users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public User? GetByEmail(string email)
        {
            var users = GetAll();
            return users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public User Create(User user)
        {
            var users = GetAll();

            user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
            user.CreatedDate = DateTime.Now;

            users.Add(user);
            SaveToFile(users);

            Console.WriteLine($"Yeni user oluşturuldu: ID={user.Id}, Username={user.Username}");
            return user;
        }

        public User Update(User user)
        {
            var users = GetAll();
            var existingIndex = users.FindIndex(u => u.Id == user.Id);

            if (existingIndex == -1)
                throw new ArgumentException($"User with ID {user.Id} not found");

            users[existingIndex] = user;
            SaveToFile(users);

            Console.WriteLine($"User güncellendi: ID={user.Id}");
            return user;
        }

        public void Delete(int id)
        {
            var users = GetAll();
            var removed = users.RemoveAll(u => u.Id == id);

            if (removed == 0)
                throw new ArgumentException($"User with ID {id} not found");

            SaveToFile(users);
            Console.WriteLine($"User silindi: ID={id}");
        }

        public bool IsUsernameExists(string username, int? excludeId = null)
        {
            var users = GetAll();
            return users.Any(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Id != excludeId);
        }

        public bool IsEmailExists(string email, int? excludeId = null)
        {
            var users = GetAll();
            return users.Any(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.Id != excludeId);
        }

        private void SaveToFile(List<User> users)
        {
            try
            {
                var directory = Path.GetDirectoryName(_usersFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(users, options);
                File.WriteAllText(_usersFilePath, json);

                Console.WriteLine($"User JSON dosyası kaydedildi: {users.Count} kayıt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"User JSON kaydetme hatası: {ex.Message}");
                throw;
            }
        }
    }
}