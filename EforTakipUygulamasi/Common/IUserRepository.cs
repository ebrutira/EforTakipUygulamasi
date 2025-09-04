using EforTakipUygulamasi.Models;

namespace EforTakipUygulamasi.Common
{
    public interface IUserRepository
    {
        List<User> GetAll();
        User? GetById(int id);
        User? GetByUsername(string username);
        User? GetByEmail(string email);
        User Create(User user);
        User Update(User user);
        void Delete(int id);
        bool IsUsernameExists(string username, int? excludeId = null);
        bool IsEmailExists(string email, int? excludeId = null);
    }
}