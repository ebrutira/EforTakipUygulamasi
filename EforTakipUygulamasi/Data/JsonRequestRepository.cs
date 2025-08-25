using System.Text.Json;
using EforTakipUygulamasi.Models;
using EforTakipUygulamasi.Common;

namespace EforTakipUygulamasi.Data
{
    public class JsonRequestRepository : IRequestRepository
    {
        private readonly string _requestsFilePath = "Data/requests.json";

        public List<Request> GetAll()
        {
            try
            {
                // Debug için
                Console.WriteLine($"JSON dosyası aranıyor: {_requestsFilePath}");
                Console.WriteLine($"Dosya var mı: {File.Exists(_requestsFilePath)}");

                if (!File.Exists(_requestsFilePath))
                {
                    Console.WriteLine("JSON dosyası bulunamadı, boş liste döndürülüyor.");
                    return new List<Request>();
                }

                var json = File.ReadAllText(_requestsFilePath);
                Console.WriteLine($"JSON içeriği: {json.Substring(0, Math.Min(200, json.Length))}...");

                var requests = JsonSerializer.Deserialize<List<Request>>(json) ?? new List<Request>();
                Console.WriteLine($"Toplam {requests.Count} request yüklendi.");

                return requests;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON okuma hatası: {ex.Message}");
                return new List<Request>();
            }
        }

        public Request? GetById(int id)
        {
            var requests = GetAll();
            return requests.FirstOrDefault(r => r.Id == id);
        }

        public Request Create(Request request)
        {
            var requests = GetAll();

            request.Id = requests.Any() ? requests.Max(r => r.Id) + 1 : 1;
            request.CreatedDate = DateTime.Now;
            request.LastModified = DateTime.Now;

            requests.Add(request);
            SaveToFile(requests);

            Console.WriteLine($"Yeni request oluşturuldu: ID={request.Id}, Name={request.Name}");

            return request;
        }

        public Request Update(Request request)
        {
            var requests = GetAll();
            var existingIndex = requests.FindIndex(r => r.Id == request.Id);

            if (existingIndex == -1)
                throw new ArgumentException($"Request with ID {request.Id} not found");

            request.LastModified = DateTime.Now;
            requests[existingIndex] = request;

            SaveToFile(requests);

            Console.WriteLine($"Request güncellendi: ID={request.Id}");

            return request;
        }

        public void Delete(int id)
        {
            var requests = GetAll();
            var removed = requests.RemoveAll(r => r.Id == id);

            if (removed == 0)
                throw new ArgumentException($"Request with ID {id} not found");

            SaveToFile(requests);

            Console.WriteLine($"Request silindi: ID={id}");
        }

        private void SaveToFile(List<Request> requests)
        {
            try
            {
                var directory = Path.GetDirectoryName(_requestsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    // camelCase KALDIR - C# property adlarını kullan
                };

                var json = JsonSerializer.Serialize(requests, options);
                File.WriteAllText(_requestsFilePath, json);

                Console.WriteLine($"JSON dosyası kaydedildi: {requests.Count} kayıt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON kaydetme hatası: {ex.Message}");
                throw;
            }
        }
    }
}
