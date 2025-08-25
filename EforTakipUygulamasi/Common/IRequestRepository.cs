using EforTakipUygulamasi.Models;

namespace EforTakipUygulamasi.Common
{
    public interface IRequestRepository
    {
        List<Request> GetAll();
        Request? GetById(int id);
        Request Create(Request request);
        Request Update(Request request);
        void Delete(int id);
    }
}