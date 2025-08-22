using EforTakipUygulamasi.Models;
using System.Text.Json;

namespace EforTakipUygulamasi.Services
{
    public class JsonRequestRepository : IRequestRepository
    {
        private readonly string _dataPath;
        private readonly string _auditPath;
        private readonly ILogger<JsonRequestRepository> _logger;
        private List<Request> _requests = new();
        private List<AuditLog> _auditLogs = new();
        private int _nextId = 1;

        public JsonRequestRepository(ILogger<JsonRequestRepository> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            var dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
            Directory.CreateDirectory(dataDirectory);

            _dataPath = Path.Combine(dataDirectory, "requests.json");
            _auditPath = Path.Combine(dataDirectory, "audit.json");

            LoadDataAsync().Wait();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                if (File.Exists(_dataPath))
                {
                    var jsonData = await File.ReadAllTextAsync(_dataPath);
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        _requests = JsonSerializer.Deserialize<List<Request>>(jsonData, options) ?? new List<Request>();
                        _nextId = _requests.Count > 0 ? _requests.Max(r => r.Id) + 1 : 1;
                    }
                }

                // Eğer hiç veri yoksa örnek veriler ekle
                if (_requests.Count == 0)
                {
                    await AddSampleDataAsync();
                }

                if (File.Exists(_auditPath))
                {
                    var auditData = await File.ReadAllTextAsync(_auditPath);
                    if (!string.IsNullOrEmpty(auditData))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        _auditLogs = JsonSerializer.Deserialize<List<AuditLog>>(auditData, options) ?? new List<AuditLog>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Veri yüklenirken hata oluştu");
            }
        }

        private async Task SaveDataAsync()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonData = JsonSerializer.Serialize(_requests, options);
                File.WriteAllTextAsync(_dataPath, jsonData);

                var auditData = JsonSerializer.Serialize(_auditLogs, options);
                await File.WriteAllTextAsync(_auditPath, auditData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Veri kaydedilirken hata oluştu");
                throw;
            }
        }

        public async Task<IEnumerable<Request>> GetAllAsync()
        {
            return await Task.FromResult(_requests.OrderByDescending(r => r.CreatedDate));
        }

        public async Task<Request?> GetByIdAsync(int id)
        {
            return await Task.FromResult(_requests.FirstOrDefault(r => r.Id == id));
        }

        public async Task<Request> AddAsync(Request request)
        {
            request.Id = _nextId++;
            request.CreatedDate = DateTime.Now;
            request.LastModified = DateTime.Now;

            _requests.Add(request);

            // Audit log ekle
            await AddAuditLogAsync("Request", request.Id, "CREATE", "", request.Name, request.CreatedBy);

            await SaveDataAsync();

            _logger.LogInformation($"Yeni request eklendi: ID={request.Id}, Name={request.Name}");

            return request;
        }

        public async Task<Request> UpdateAsync(Request request)
        {
            var existingRequest = _requests.FirstOrDefault(r => r.Id == request.Id);
            if (existingRequest == null)
                throw new ArgumentException("Talep bulunamadı");

            // Audit için eski değerleri karşılaştır
            await CompareAndAuditChanges(existingRequest, request);

            var index = _requests.FindIndex(r => r.Id == request.Id);
            request.LastModified = DateTime.Now;
            _requests[index] = request;

            await SaveDataAsync();

            _logger.LogInformation($"Request güncellendi: ID={request.Id}, Name={request.Name}");

            return request;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var request = _requests.FirstOrDefault(r => r.Id == id);
            if (request == null) return false;

            _requests.RemoveAll(r => r.Id == id);

            // Audit log ekle
            await AddAuditLogAsync("Request", id, "DELETE", request.Name, "", "System");

            await SaveDataAsync();

            _logger.LogInformation($"Request silindi: ID={id}, Name={request.Name}");

            return true;
        }

        private async Task AddSampleDataAsync()
        {
            var sampleData = new List<Request>
            {
                new Request
                {
                    Id = 1,
                    Name = "Kullanıcı Giriş Sistemi",
                    Description = "Login/logout ve şifre sıfırlama modülü",
                    Status = RequestStatus.InProgress,
                    Priority = RequestPriority.High,
                    AnalystHours = 8,
                    DeveloperHours = 16,
                    KKTHours = 4,
                    PreprodHours = 2,
                    Deadline = DateTime.Today.AddDays(7),
                    CreatedBy = "Koray",
                    LastModifiedBy = "Koray",
                    CreatedDate = DateTime.Now.AddDays(-5),
                    LastModified = DateTime.Now.AddDays(-1)
                },
                new Request
                {
                    Id = 2,
                    Name = "Rapor Modülü",
                    Description = "Aylık ve haftalık raporlama sistemi",
                    Status = RequestStatus.New,
                    Priority = RequestPriority.Medium,
                    AnalystHours = 12,
                    DeveloperHours = 24,
                    KKTHours = 6,
                    PreprodHours = 3,
                    Deadline = DateTime.Today.AddDays(14),
                    CreatedBy = "Koray",
                    LastModifiedBy = "Koray",
                    CreatedDate = DateTime.Now.AddDays(-3),
                    LastModified = DateTime.Now.AddDays(-3)
                },
                new Request
                {
                    Id = 3,
                    Name = "Dashboard Tasarımı",
                    Description = "Ana sayfa dashboard geliştirme",
                    Status = RequestStatus.Testing,
                    Priority = RequestPriority.Medium,
                    AnalystHours = 4,
                    DeveloperHours = 12,
                    KKTHours = 2,
                    PreprodHours = 1,
                    Deadline = DateTime.Today.AddDays(-2), // Gecikmiş
                    CreatedBy = "Koray",
                    LastModifiedBy = "Koray",
                    CreatedDate = DateTime.Now.AddDays(-10),
                    LastModified = DateTime.Now.AddDays(-2)
                },
                new Request
                {
                    Id = 4,
                    Name = "Veritabanı Optimizasyonu",
                    Description = "SQL performans iyileştirmeleri",
                    Status = RequestStatus.Completed,
                    Priority = RequestPriority.Critical,
                    AnalystHours = 16,
                    DeveloperHours = 20,
                    KKTHours = 6,
                    PreprodHours = 4,
                    Deadline = DateTime.Today.AddDays(-5),
                    CreatedBy = "Koray",
                    LastModifiedBy = "Koray",
                    CreatedDate = DateTime.Now.AddDays(-15),
                    LastModified = DateTime.Now.AddDays(-5)
                },
                new Request
                {
                    Id = 5,
                    Name = "API Geliştirme",
                    Description = "REST API servisleri geliştirme",
                    Status = RequestStatus.InProgress,
                    Priority = RequestPriority.High,
                    AnalystHours = 6,
                    DeveloperHours = 18,
                    KKTHours = 4,
                    PreprodHours = 2,
                    Deadline = DateTime.Today.AddDays(10),
                    CreatedBy = "Koray",
                    LastModifiedBy = "Koray",
                    CreatedDate = DateTime.Now.AddDays(-2),
                    LastModified = DateTime.Now.AddDays(-1)
                },
                new Request
                {
                    Id = 6,
                    Name = "Mobile App Frontend",
                    Description = "React Native mobil uygulama arayüzü",
                    Status = RequestStatus.New,
                    Priority = RequestPriority.Medium,
                    AnalystHours = 10,
                    DeveloperHours = 35,
                    KKTHours = 8,
                    PreprodHours = 5,
                    Deadline = DateTime.Today.AddDays(21),
                    CreatedBy = "Koray",
                    LastModifiedBy = "Koray",
                    CreatedDate = DateTime.Now.AddDays(-1),
                    LastModified = DateTime.Now.AddDays(-1)
                },
                new Request
                {
                    Id = 7,
                    Name = "Email Entegrasyonu",
                    Description = "SMTP email gönderme sistemi",
                    Status = RequestStatus.Testing,
                    Priority = RequestPriority.Low,
                    AnalystHours = 2,
                    DeveloperHours = 8,
                    KKTHours = 2,
                    PreprodHours = 1,
                    Deadline = DateTime.Today.AddDays(5),
                    CreatedBy = "Koray",
                    LastModifiedBy = "Koray",
                    CreatedDate = DateTime.Now.AddDays(-8),
                    LastModified = DateTime.Now.AddDays(-3)
                },
                new Request
                {
                    Id = 8,
                    Name = "Yedekleme Sistemi",
                    Description = "Otomatik veritabanı yedekleme",
                    Status = RequestStatus.Completed,
                    Priority = RequestPriority.Critical,
                    AnalystHours = 4,
                    DeveloperHours = 12,
                    KKTHours = 3,
                    PreprodHours = 2,
                    Deadline = DateTime.Today.AddDays(-10),
                    CreatedBy = "Koray",
                    LastModifiedBy = "Koray",
                    CreatedDate = DateTime.Now.AddDays(-20),
                    LastModified = DateTime.Now.AddDays(-10)
                },
                new Request
                {
                    Id = 9,
                    Name = "Güvenlik Güncellemeleri",
                    Description = "SSL ve güvenlik yamalarının uygulanması",
                    Status = RequestStatus.InProgress,
                    Priority = RequestPriority.Critical,
                    AnalystHours = 3,
                    DeveloperHours = 6,
                    KKTHours = 4,
                    PreprodHours = 3,
                    Deadline = DateTime.Today.AddDays(3),
                    CreatedBy = "Koray",
                    LastModifiedBy = "Koray",
                    CreatedDate = DateTime.Now.AddDays(-4),
                    LastModified = DateTime.Now
                },
                new Request
                {
                    Id = 10,
                    Name = "Performans İzleme",
                    Description = "APM ve monitoring araçları kurulumu",
                    Status = RequestStatus.New,
                    Priority = RequestPriority.Medium,
                    AnalystHours = 8,
                    DeveloperHours = 16,
                    KKTHours = 6,
                    PreprodHours = 4,
                    Deadline = DateTime.Today.AddDays(15),
                    CreatedBy = "Koray",
                    LastModifiedBy = "Koray",
                    CreatedDate = DateTime.Now,
                    LastModified = DateTime.Now
                }
            };

            _requests.AddRange(sampleData);
            _nextId = 11;

            // Audit logları ekle
            foreach (var request in sampleData)
            {
                await AddAuditLogAsync("Request", request.Id, "CREATE", "", request.Name, request.CreatedBy);
            }

            await SaveDataAsync();
        }

        public async Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status)
        {
            return await Task.FromResult(_requests.Where(r => r.Status == status));
        }

        public async Task<IEnumerable<Request>> GetOverdueAsync()
        {
            var today = DateTime.Today;
            return await Task.FromResult(_requests.Where(r =>
                r.Deadline.HasValue &&
                r.Deadline.Value.Date < today &&
                r.Status != RequestStatus.Completed &&
                r.Status != RequestStatus.Cancelled));
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var stats = new DashboardStats
            {
                TotalRequests = _requests.Count,
                InProgressRequests = _requests.Count(r => r.Status == RequestStatus.InProgress),
                TotalHours = _requests.Sum(r => r.TotalHours)
            };

            var overdue = await GetOverdueAsync();
            stats.OverdueRequests = overdue.Count();

            // T-Shirt size dağılımı
            foreach (TShirtSize size in Enum.GetValues<TShirtSize>())
            {
                stats.SizeDistribution[size] = _requests.Count(r => r.Size == size);
            }

            // Status dağılımı
            foreach (RequestStatus status in Enum.GetValues<RequestStatus>())
            {
                stats.StatusDistribution[status] = _requests.Count(r => r.Status == status);
            }

            return stats;
        }

        private async Task CompareAndAuditChanges(Request oldRequest, Request newRequest)
        {
            var user = newRequest.LastModifiedBy;

            if (oldRequest.Name != newRequest.Name)
                await AddAuditLogAsync("Request", newRequest.Id, "UPDATE", oldRequest.Name, newRequest.Name, user, "Name");

            if (oldRequest.Status != newRequest.Status)
                await AddAuditLogAsync("Request", newRequest.Id, "UPDATE", oldRequest.Status.ToString(), newRequest.Status.ToString(), user, "Status");

            if (oldRequest.AnalystHours != newRequest.AnalystHours)
                await AddAuditLogAsync("Request", newRequest.Id, "UPDATE", oldRequest.AnalystHours.ToString(), newRequest.AnalystHours.ToString(), user, "AnalystHours");

            if (oldRequest.DeveloperHours != newRequest.DeveloperHours)
                await AddAuditLogAsync("Request", newRequest.Id, "UPDATE", oldRequest.DeveloperHours.ToString(), newRequest.DeveloperHours.ToString(), user, "DeveloperHours");
        }

        private async Task AddAuditLogAsync(string entity, int entityId, string action, string oldValue, string newValue, string user, string field = "")
        {
            var auditLog = new AuditLog
            {
                Id = _auditLogs.Count + 1,
                Entity = entity,
                EntityId = entityId,
                Field = field,
                OldValue = oldValue,
                NewValue = newValue,
                User = user,
                Date = DateTime.Now,
                Action = action
            };
        }

        public async Task<bool> ResetDataAsync()
        {
            try
            {
                _requests.Clear();
                _auditLogs.Clear();
                await AddSampleDataAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Veri sıfırlanırken hata oluştu");
                return false;
            }
        }
    }
}