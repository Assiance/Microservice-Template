using System.Threading.Tasks;

namespace EfMicroservice.Persistence.Clients.Interfaces
{
    public interface IGitHaubService
    {
        Task<object> Get();
    }
}
